using CryptoService.Clients.Interfaces;
using CryptoService.DTOs;
using CryptoService.Models;
using CryptoService.Persistance;
using CryptoService.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using NBitcoin;

namespace CryptoService.Services;

public class CryptoPaymentService : ICryptoPaymentService
{
    private readonly CryptoDbContext _db;
    private readonly HttpClient _httpClient;
    private readonly IBinanceClient _binanceClient;
    private readonly ITestnetWallet _testnetWallet;

    private const string BlockstreamTestnetBase = "https://blockstream.info/testnet/api";
    private readonly BitcoinSecret _shopWalletSecret;
    private readonly BitcoinAddress _shopWalletAddress;

    public CryptoPaymentService(CryptoDbContext db, HttpClient httpClient, IBinanceClient binanceClient, ITestnetWallet testnetWallet, IConfiguration config)
    {
        _db = db;
        _httpClient = httpClient;
        _binanceClient = binanceClient;
        _testnetWallet = testnetWallet;
        _shopWalletSecret = new BitcoinSecret(config["BitcoinTestnetWalletWif"], Network.TestNet); // TODO: Make method that generates a wallet and a secret, save both
        _shopWalletAddress = _shopWalletSecret.GetAddress(ScriptPubKeyType.Segwit);
    }

    /// <summary>
    /// Generate a payment record, transaction gets associated with this later
    /// </summary>
    public async Task<CreateCryptoPaymentResponse> CreatePaymentAsync(CreateCryptoPaymentRequest request, CancellationToken cancellationToken)
    {
        string symbol = GetBinanceSymbol(request.FiatCurrency);
        decimal btcPrice = await _binanceClient.GetBitcoinPriceAsync(symbol, cancellationToken);

        decimal btcAmount = decimal.Round(request.FiatAmount / btcPrice, 8);

        // 2. Generate testnet address
        Key key = new Key();
        BitcoinAddress address = key.PubKey.GetAddress(ScriptPubKeyType.Segwit, Network.TestNet);

        // 3. Store payment
        var payment = new CryptoPayment
        {
            Id = Guid.NewGuid(),
            OrderId = request.OrderId,
            FiatAmount = request.FiatAmount,
            FiatCurrency = request.FiatCurrency,
            BitcoinAmount = decimal.Round(btcAmount, 8),
            BitcoinAddress = address.ToString(),
            Status = CryptoPaymentStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddMinutes(20)
        };

        _db.CryptoPayments.Add(payment);
        await _db.SaveChangesAsync(cancellationToken);

        return new CreateCryptoPaymentResponse(
            payment.Id,
            payment.BitcoinAddress,
            payment.BitcoinAmount,
            payment.ExpiresAt);
    }

    public async Task<CryptoPaymentStatusResponse?> GetStatusAsync(Guid paymentId, CancellationToken cancellationToken)
    {
        CryptoPayment? payment = await _db.CryptoPayments.FirstOrDefaultAsync(x => x.Id == paymentId, cancellationToken);

        if (payment is null)
        {
            return null;
        }

        int confirmations = 0;

        if (payment.TransactionId is not null)
        {
            BitcoinTransactionDto? tx = await _httpClient.GetFromJsonAsync<BitcoinTransactionDto>(
                    $"{BlockstreamTestnetBase}/tx/{payment.TransactionId}",
                    cancellationToken);

            if (tx?.Status.Confirmed == true &&
                tx.Status.BlockHeight.HasValue)
            {
                confirmations = 1; // simplified
                payment.Status = CryptoPaymentStatus.Confirmed;
            }
        }

        return new CryptoPaymentStatusResponse(
            payment.Id,
            payment.Status,
            payment.BitcoinAmount,
            payment.TransactionId,
            confirmations);
    }
    private string GetBinanceSymbol(Currency currency) => currency switch
    {
        Currency.USD => "BTCUSDT",
        Currency.EUR => "BTCEUR",
        Currency.GBP => "BTCGBP",
        Currency.CHF => "BTCCHF",
        Currency.JPY => "BTCJPY",
        _ => throw new Exception($"Currency {currency} not supported by Binance")
    };

    /// <summary>
    /// Simulate sending BTC, generates a mock transaction ID and marks payment as confirmed
    /// </summary>

    public async Task<string> ProcessPaymentAsync(Guid paymentId, CancellationToken cancellationToken)
    {
        var payment = await _db.CryptoPayments.FirstOrDefaultAsync(x => x.Id == paymentId, cancellationToken);
        if (payment is null) throw new Exception("Payment not found");
        if (payment.Status != CryptoPaymentStatus.Pending)
            throw new InvalidOperationException("Payment is already processed");

        // 1. Generate or load a funded testnet wallet (you must fund it via a Testnet faucet)
        (BitcoinSecret secret, BitcoinAddress studentAddress) = _testnetWallet.GenerateWallet();
        Console.WriteLine($"Fund this testnet address using a faucet: {studentAddress}");

        string txId = await _testnetWallet.SendPaymentAsync(
            _shopWalletSecret,
            payment.BitcoinAddress,
            payment.BitcoinAmount,
            cancellationToken
        );

        payment.TransactionId = txId;
        payment.Status = CryptoPaymentStatus.Confirmed;
        await _db.SaveChangesAsync(cancellationToken);

        Console.WriteLine($"Payment processed. TXID: {txId}");
        return txId;
    }


    /// <summary>
    /// Checks transaction status on testnet API (Blockstream)
    /// </summary>
    public async Task<CryptoPaymentStatusResponse?> CheckPaymentStatusAsync(Guid paymentId, CancellationToken cancellationToken)
    {
        CryptoPayment? payment = await _db.CryptoPayments.FirstOrDefaultAsync(x => x.Id == paymentId, cancellationToken);
        if (payment is null) return null;

        int confirmations = 0;

        if (!string.IsNullOrEmpty(payment.TransactionId))
        {
            try
            {
                bool confirmed = await _testnetWallet.IsConfirmedAsync(payment.TransactionId);

                if (confirmed)
                {
                    confirmations = 1; // simplified for school project
                    payment.Status = CryptoPaymentStatus.Confirmed;
                    await _db.SaveChangesAsync(cancellationToken);
                }
            }
            catch
            {
                // Transaction not found yet on testnet, keep pending
            }
        }
        return new CryptoPaymentStatusResponse(
            payment.Id,
            payment.Status,
            payment.BitcoinAmount,
            payment.TransactionId,
            confirmations);
    }

    public async Task<GenerateWalletResponse> GenerateWalletAsync()
    {
        var (wif, address) = _testnetWallet.GenerateWifWallet();
        return new GenerateWalletResponse(wif, address);
    }
}
