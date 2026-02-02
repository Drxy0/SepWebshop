using Bank.Clients;
using Bank.Contracts;
using Bank.Contracts.QR;
using Bank.Helpers;
using Bank.Models;
using Bank.Persistance;
using Bank.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Bank.Services;

public class PaymentService : IPaymentService
{
    private readonly BankDbContext _context;
    private readonly string _frontendUrl;
    private IPSPClient _pspClient;
    private readonly Dictionary<string, string> _pspHmacKeys; // temp


    public PaymentService(BankDbContext context, IConfiguration config, IPSPClient pspClient)
    {
        _context = context;
        _frontendUrl = config["BankFrontendUrl"]!;
        _pspClient = pspClient;
        _pspHmacKeys = config.GetSection("PSPHmacKeys").Get<Dictionary<string, string>>() ?? new (); // temp
    }

    public async Task<InitializePaymentServiceResult> InitializePayment(PaymentInitRequest request, Guid pspId, string signature, DateTime timestamp, bool isQrPayment)
    {
        // Validate PSP
        PSP? psp = await _context.PSPs.FindAsync(pspId);
        if (psp is null)
        {
            return new InitializePaymentServiceResult(InitializePaymentResult.InvalidPsp, null);
        }

        // Get HMAC key
        if (!_pspHmacKeys.TryGetValue(pspId.ToString(), out var hmacKey))
            return new InitializePaymentServiceResult(InitializePaymentResult.InvalidPsp, null);

        // Validate signature
        string payload =
            $"merchantId={request.MerchantId}&amount={request.Amount}" +
            $"&currency={(int)request.Currency}&stan={request.Stan}" +
            $"&timestamp={timestamp:o}";

        // Validate merchant
        Merchant? merchant = await _context.Merchants.FindAsync(request.MerchantId);
        if (merchant is null)
        {
            return new InitializePaymentServiceResult(InitializePaymentResult.InvalidMerchant, null);
        }

        PaymentRequest paymentRequest = new PaymentRequest
        {
            PaymentRequestId = Guid.NewGuid(),
            MerchantId = request.MerchantId,
            PspId = pspId,
            PspPaymentId = request.PspPaymentId,
            Amount = request.Amount,
            Currency = request.Currency,
            Stan = request.Stan,
            PspTimestamp = request.PspTimestamp,
            Status = PaymentRequestStatus.Pending,
            ExpiresAt = DateTime.UtcNow.AddMinutes(15)
        };

        _context.PaymentRequests.Add(paymentRequest);
        await _context.SaveChangesAsync();

        string paymentUrl = isQrPayment
            ? $"{_frontendUrl}/pay/qr/{paymentRequest.PaymentRequestId}"
            : $"{_frontendUrl}/pay/card/{paymentRequest.PaymentRequestId}";

        return new InitializePaymentServiceResult(
            Result: InitializePaymentResult.Success,
            Response: new InitPaymentResponseDto(paymentRequest.PaymentRequestId, paymentUrl)
        );
    }

    public async Task<string> ExecuteCardPayment(Guid paymentRequestId, PayByCardRequest request)
    {
        using var dbTransaction = await _context.Database.BeginTransactionAsync();

        PaymentRequest? paymentRequest = await _context.PaymentRequests
            .FirstOrDefaultAsync(p => p.PaymentRequestId == paymentRequestId);

        if (paymentRequest == null || paymentRequest.Status != PaymentRequestStatus.Pending)
        {
            return await NotifyFailure(paymentRequestId, TransactionStatus.Failed);
        }

        if (paymentRequest.ExpiresAt < DateTime.UtcNow)
        {
            paymentRequest.Status = PaymentRequestStatus.Expired;
            await _context.SaveChangesAsync();
            await dbTransaction.CommitAsync();
            return await NotifyFailure(paymentRequestId, TransactionStatus.Failed);
        }

        if (string.IsNullOrWhiteSpace(request.CVV) || request.CVV.Length != 3 || !request.CVV.All(char.IsDigit))
        {
            paymentRequest.Status = PaymentRequestStatus.Failed;
            await _context.SaveChangesAsync();
            await dbTransaction.CommitAsync();

            return await NotifyFailure(paymentRequestId, TransactionStatus.Failed);
        }

        // Card validation
        if (!LuhnFormulaChecker.IsValidLuhn(request.CardNumber))
        {
            paymentRequest.Status = PaymentRequestStatus.Failed;
            await _context.SaveChangesAsync();
            await dbTransaction.CommitAsync();

            return await NotifyFailure(paymentRequestId, TransactionStatus.Failed);
        }

        DebitCard? card = await _context.DebitCards
            .Include(c => c.Account)
            .FirstOrDefaultAsync(c => c.CardNumber == request.CardNumber);

        if (card == null)
        {
            return await NotifyFailure(paymentRequestId, TransactionStatus.Failed);
        }

        // Check if request expiry matches card expiry
        string requestExpiry = $"{request.ExpiryMonth:D2}/{request.ExpiryYear:D2}";
        
        if (DebitCardHelper.IsCardExpired(card.ExpirationDate) || 
            card.ExpirationDate != requestExpiry ||
            card.CVV != request.CVV || 
            card.Account.Balance < paymentRequest.Amount)
        {
            paymentRequest.Status = PaymentRequestStatus.Failed;
            await _context.SaveChangesAsync();
            await dbTransaction.CommitAsync();

            return await NotifyFailure(paymentRequestId, TransactionStatus.Failed);
        }

        var merchant = await _context.Merchants
            .Include(c => c.Account)
            .FirstOrDefaultAsync(b => b.Id == paymentRequest.MerchantId);


        if (merchant?.Account is null)
        {
            paymentRequest.Status = PaymentRequestStatus.Failed;
            await _context.SaveChangesAsync();
            await dbTransaction.CommitAsync();

            return await NotifyFailure(paymentRequestId, TransactionStatus.Failed);
        }

        try
        {
            card.Account.Balance -= paymentRequest.Amount;
            merchant.Account.Balance += paymentRequest.Amount;

            var globalTransactionId = Guid.NewGuid();
            var acquirerTimestamp = DateTime.UtcNow;

            _context.Transactions.Add(new Transaction
            {
                PaymentRequestId = paymentRequestId,
                GlobalTransactionId = globalTransactionId,
                AcquirerTimestamp = acquirerTimestamp,
                Status = TransactionStatus.Successful
            });

            paymentRequest.Status = PaymentRequestStatus.Success;

            await _context.SaveChangesAsync();
            await dbTransaction.CommitAsync();

            var redirectUrl = await _pspClient.NotifyPaymentStatusAsync(new PspPaymentStatusDto
            {
                PaymentRequestId = paymentRequestId,
                PspId = paymentRequest.PspId,
                PspPaymentId = paymentRequest.PspPaymentId,
                Stan = paymentRequest.Stan,
                GlobalTransactionId = globalTransactionId,
                AcquirerTimestamp = acquirerTimestamp,
                Status = TransactionStatus.Successful,
                MerchantId = merchant.Id,
                PspTimestamp = paymentRequest.PspTimestamp,
            });

            return redirectUrl;
        }
        catch
        {
            await dbTransaction.RollbackAsync();

            var globalTransactionId = Guid.NewGuid();
            var acquirerTimestamp = DateTime.UtcNow;

            _context.Transactions.Add(new Transaction
            {
                PaymentRequestId = paymentRequestId,
                GlobalTransactionId = globalTransactionId,
                AcquirerTimestamp = acquirerTimestamp,
                Status = TransactionStatus.Failed
            });

            await _context.SaveChangesAsync();

            return await NotifyFailure(paymentRequestId, TransactionStatus.Failed);
        }
    }

    private async Task<string> NotifyFailure(Guid paymentRequestId, TransactionStatus status)
    {
        PaymentRequest? paymentRequest = await _context.PaymentRequests.FindAsync(paymentRequestId);

        if (paymentRequest is null)
        {
            throw new Exception("Payment request not found.");
        }

        var redirectUrl = await _pspClient.NotifyPaymentStatusAsync(new PspPaymentStatusDto
        {
            PaymentRequestId = paymentRequestId,
            PspId = paymentRequest.PspId,
            PspPaymentId = paymentRequest.PspPaymentId,
            Stan = paymentRequest.Stan,
            GlobalTransactionId = Guid.NewGuid(),
            AcquirerTimestamp = DateTime.UtcNow,
            Status = status,
            MerchantId = paymentRequest.MerchantId,
            PspTimestamp = paymentRequest.PspTimestamp
        });

        return redirectUrl;
    }

    public async Task<PaymentRequestDto> GetPaymentRequest(Guid paymentRequestId)
    {
        PaymentRequestDto? paymentRequest = await _context.PaymentRequests
            .Where(p => p.PaymentRequestId == paymentRequestId)
            .Select(p => new PaymentRequestDto(
                p.PaymentRequestId,p.Amount,p.Currency,p.Status, p.ExpiresAt))
            .FirstOrDefaultAsync();

        if (paymentRequest == null)
        {
            throw new Exception("Payment request not found.");
        }

        if (paymentRequest.Status != PaymentRequestStatus.Pending)
        {
            throw new Exception("Payment request is not valid.");
        }

        if (paymentRequest.ExpiresAt < DateTime.UtcNow)
        {
            throw new Exception("Payment request expired.");
        }

        return paymentRequest;
    }

    public async Task<QRPaymentResponseDto> GenerateQrPayment(Guid paymentRequestId)
    {
        PaymentRequest? paymentRequest = await _context.PaymentRequests
                .Include(p => p.Merchant)
                .ThenInclude(m => m.Account)
                .FirstOrDefaultAsync(p => p.PaymentRequestId == paymentRequestId);

        if (paymentRequest == null)
            throw new Exception("Payment request not found");

        var ipsData = new QRIpsData(
            Currency: paymentRequest.Currency,
            Amount: paymentRequest.Amount,
            MerchantAccount: paymentRequest.Merchant.Account.AccountNumber.Replace("-", ""),
            MerchantName: paymentRequest.Merchant.Name,
            Purpose: "Placanje robe",
            PaymentCode: "289",
            Stan: paymentRequest.Stan
        );

        var payload = QRIpsPayloadGenerator.Generate(ipsData);
        var qrBase64 = QrImageGenerator.GenerateBase64(payload);

        return new QRPaymentResponseDto(
            paymentRequestId,
            qrBase64,
            paymentRequest.Status,
            paymentRequest.Stan,
            paymentRequest.ExpiresAt
        );
    }

    // This method simulates what would happen when a real customer scans and pays via their banking app
    // In reality, IPS (National Bank of Serbia service) would trigger the ProcessIpsCallback method
    public async Task<QRPaymentResponseDto> ProcessQrPayment(Guid paymentRequestId, string? customerAccountNumber = null)
    {
        var paymentRequest = await _context.PaymentRequests
            .Include(p => p.Merchant)
                .ThenInclude(m => m.Account)
            .FirstOrDefaultAsync(p => p.PaymentRequestId == paymentRequestId);

        if (paymentRequest == null)
            throw new Exception("Payment request not found");

        // Check if already processed
        if (paymentRequest.Status != PaymentRequestStatus.Pending)
        {
            return new QRPaymentResponseDto(
                paymentRequestId,
                null,
                paymentRequest.Status,
                paymentRequest.Stan,
                paymentRequest.ExpiresAt
            );
        }

        // Check if expired
        if (paymentRequest.ExpiresAt < DateTime.UtcNow)
        {
            paymentRequest.Status = PaymentRequestStatus.Expired;
            await _context.SaveChangesAsync();
            return new QRPaymentResponseDto(
                paymentRequestId,
                null,
                PaymentRequestStatus.Expired,
                paymentRequest.Stan,
                paymentRequest.ExpiresAt
            );
        }

        // Complete payment
        return await CompleteQrPayment(paymentRequestId, customerAccountNumber);
    }

    private async Task<QRPaymentResponseDto> CompleteQrPayment(Guid paymentRequestId, string? customerAccountNumber)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var paymentRequest = await _context.PaymentRequests
                .Include(p => p.Merchant)
                    .ThenInclude(m => m.Account)
                .FirstOrDefaultAsync(p => p.PaymentRequestId == paymentRequestId);

            if (paymentRequest is null || paymentRequest.Status != PaymentRequestStatus.Pending)
            {
                throw new Exception("Payment request not valid");
            }

            // Find customer account for deduction
            Account? customerAccount = null;
            if (!string.IsNullOrEmpty(customerAccountNumber))
            {
                customerAccount = await _context.Accounts
                    .FirstOrDefaultAsync(a => a.AccountNumber == customerAccountNumber);
            }

            // If no specific account or not found, use a default for simulation
            if (customerAccount == null)
            {
                // Create or use a test customer account
                customerAccount = await _context.Accounts
                    .FirstOrDefaultAsync(a => a.Type == AccountType.Customer && a.Balance >= paymentRequest.Amount);

                if (customerAccount == null)
                {
                    // Create test customer account
                    customerAccount = new Account
                    {
                        Id = Guid.NewGuid(),
                        AccountNumber = "123-456789-78",
                        AccountHolderName = "Test Customer",
                        Balance = 100000,
                        Type = AccountType.Customer
                    };
                    _context.Accounts.Add(customerAccount);
                }
            }

            // Check balance
            if (customerAccount.Balance < paymentRequest.Amount)
            {
                paymentRequest.Status = PaymentRequestStatus.Failed;
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return new QRPaymentResponseDto(
                    paymentRequestId,
                    null,
                    PaymentRequestStatus.Failed,
                    paymentRequest.Stan,
                    paymentRequest.ExpiresAt
                );
            }

            // Transfer funds
            customerAccount.Balance -= paymentRequest.Amount;
            paymentRequest.Merchant.Account.Balance += paymentRequest.Amount;

            // Record transaction
            var globalTransactionId = Guid.NewGuid();
            var acquirerTimestamp = DateTime.UtcNow;

            _context.Transactions.Add(new Transaction
            {
                PaymentRequestId = paymentRequestId,
                GlobalTransactionId = globalTransactionId,
                AcquirerTimestamp = acquirerTimestamp,
                Status = TransactionStatus.Successful,
                Reference = $"QR-PAY-{Guid.NewGuid()}",
                Description = $"QR payment completed via mobile app"
            });

            paymentRequest.Status = PaymentRequestStatus.Success;

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            // Notify PSP
            string? redirectUrl = null;
            try
            {
                redirectUrl = await _pspClient.NotifyPaymentStatusAsync(new PspPaymentStatusDto
                {
                    PaymentRequestId = paymentRequestId,
                    PspId = paymentRequest.PspId,
                    PspPaymentId = paymentRequest.PspPaymentId,
                    Stan = paymentRequest.Stan,
                    GlobalTransactionId = globalTransactionId,
                    AcquirerTimestamp = acquirerTimestamp,
                    Status = TransactionStatus.Successful,
                    MerchantId = paymentRequest.MerchantId,
                    PspTimestamp = paymentRequest.PspTimestamp,
                });
            }
            catch(Exception ex)
            {
                Console.WriteLine($"\n\n------------------------\nError notifying PSP: {ex.Message}");
            }

            return new QRPaymentResponseDto(
                paymentRequestId,
                null,
                PaymentRequestStatus.Success,
                paymentRequest.Stan,
                paymentRequest.ExpiresAt,
                redirectUrl
            );
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    // This is the callback that would be triggered by IPS (National Bank of Serbia)
    // when a real customer completes payment through their official banking app
    public async Task ProcessIpsCallback(IpsCallbackDto callbackData)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            // Find payment request by STAN (used as reference)
            var paymentRequest = await _context.PaymentRequests
                .Include(p => p.Merchant)
                    .ThenInclude(m => m.Account)
                .FirstOrDefaultAsync(p => p.Stan == callbackData.Reference);

            if (paymentRequest == null)
                throw new Exception($"Payment request not found for reference: {callbackData.Reference}");

            if (paymentRequest.Status != PaymentRequestStatus.Pending)
            {
                await transaction.CommitAsync();
                return; // Already processed
            }

            if (callbackData.Status == IpsPaymentStatus.Success)
            {
                // Handle successful payment from IPS
                if (!string.IsNullOrEmpty(callbackData.PayerAccountNumber))
                {
                    var customerAccount = await _context.Accounts
                        .FirstOrDefaultAsync(a => a.AccountNumber == callbackData.PayerAccountNumber);

                    if (customerAccount != null && customerAccount.Balance >= paymentRequest.Amount)
                    {
                        customerAccount.Balance -= paymentRequest.Amount;
                    }
                    else
                    {
                        throw new Exception("Customer account not found or insufficient balance");
                    }
                }
                else
                {
                    // Simulate: find any customer account with sufficient balance
                    Account? customerAccount = await _context.Accounts
                        .Where(a => a.Type == AccountType.Customer && a.Balance >= paymentRequest.Amount)
                        .FirstOrDefaultAsync();

                    if (customerAccount != null)
                    {
                        customerAccount.Balance -= paymentRequest.Amount;
                    }
                    else
                    {
                        // Create simulation account if none exists
                        Account defaultCustomer = new Account
                        {
                            AccountNumber = "123-456789-78",
                            AccountHolderName = "Test Customer",
                            Balance = 100000,
                            Type = AccountType.Customer
                        };

                        _context.Accounts.Add(defaultCustomer);
                        defaultCustomer.Balance -= paymentRequest.Amount;
                    }
                }

                // Credit merchant
                paymentRequest.Merchant.Account.Balance += paymentRequest.Amount;

                // Record transaction
                _context.Transactions.Add(new Transaction
                {
                    PaymentRequestId = paymentRequest.PaymentRequestId,
                    GlobalTransactionId = Guid.NewGuid(),
                    AcquirerTimestamp = DateTime.UtcNow,
                    Status = TransactionStatus.Successful,
                    Reference = callbackData.TransactionId,
                    Description = $"IPS QR Payment completed"
                });

                paymentRequest.Status = PaymentRequestStatus.Success;
            }
            else
            {
                // Record failed transaction
                _context.Transactions.Add(new Transaction
                {
                    PaymentRequestId = paymentRequest.PaymentRequestId,
                    GlobalTransactionId = Guid.NewGuid(),
                    AcquirerTimestamp = DateTime.UtcNow,
                    Status = TransactionStatus.Failed,
                    Reference = callbackData.TransactionId,
                    Description = $"IPS QR Payment failed: {callbackData.Reason}"
                });

                paymentRequest.Status = PaymentRequestStatus.Failed;
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            // Notify PSP about the result
            await _pspClient.NotifyPaymentStatusAsync(new PspPaymentStatusDto
            {
                PaymentRequestId = paymentRequest.PaymentRequestId,
                PspId = paymentRequest.PspId,
                PspPaymentId = paymentRequest.PspPaymentId,
                Stan = paymentRequest.Stan,
                GlobalTransactionId = Guid.NewGuid(),
                AcquirerTimestamp = DateTime.UtcNow,
                Status = callbackData.Status == IpsPaymentStatus.Success
                            ? TransactionStatus.Successful
                            : TransactionStatus.Failed,
                MerchantId = paymentRequest.MerchantId,
                PspTimestamp = paymentRequest.PspTimestamp,
            });
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    // Bank frontend calls this method to poll for payment status updates
    public async Task<QrPaymentStatusDto> GetQrPaymentStatus(Guid paymentRequestId)
    {
        PaymentRequest? paymentRequest = await _context.PaymentRequests
            .Include(p => p.Transactions)
            .FirstOrDefaultAsync(p => p.PaymentRequestId == paymentRequestId);

        if (paymentRequest == null)
            throw new Exception("Payment request not found.");

        var latestTransaction = paymentRequest.Transactions
            .OrderByDescending(t => t.AcquirerTimestamp)
            .FirstOrDefault();

        return new QrPaymentStatusDto(
            PaymentRequestId: paymentRequest.PaymentRequestId,
            Status: paymentRequest.Status,
            Amount: paymentRequest.Amount,
            Currency: paymentRequest.Currency,
            ExpiresAt: paymentRequest.ExpiresAt,
            TransactionId: latestTransaction?.Reference,
            CompletedAt: latestTransaction?.AcquirerTimestamp
        );
    }
}