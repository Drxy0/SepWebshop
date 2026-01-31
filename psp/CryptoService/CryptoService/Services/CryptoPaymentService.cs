using CryptoService.Clients.Interfaces;
using CryptoService.DTOs;
using CryptoService.Models;
using CryptoService.Persistance;
using CryptoService.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using NBitcoin;
using QRCoder;

namespace CryptoService.Services;

public class CryptoPaymentService : ICryptoPaymentService
{
    private readonly CryptoDbContext _db;
    private readonly HttpClient _httpClient;
    private readonly IBinanceClient _binanceClient;
    private readonly IWebshopClient _webshopClient;

    private readonly BitcoinSecret _shopWalletSecret;
    private readonly BitcoinAddress _shopWalletAddress;

    private readonly string _blockstreamApiUrl;

    public CryptoPaymentService(CryptoDbContext db, HttpClient httpClient, IBinanceClient binanceClient, IConfiguration config, IWebshopClient webshopClient)
    {
        _db = db;
        _httpClient = httpClient;
        _binanceClient = binanceClient;
        _webshopClient = webshopClient;

        // Load Blockstream URL from config
        _blockstreamApiUrl = config["BlockstreamUrl"] ?? throw new InvalidOperationException("BlockstreamUrl not configured");

        string wif = config["TestShopWallet:Wif"] ?? throw new InvalidOperationException("TestShopWallet:Wif not configured");

        _shopWalletSecret = new BitcoinSecret(wif, Network.TestNet);
        _shopWalletAddress = _shopWalletSecret.GetAddress(ScriptPubKeyType.Segwit);

        string configuredAddress = config["TestShopWallet:Address"] ?? string.Empty;
        if (!string.IsNullOrEmpty(configuredAddress) &&
            _shopWalletAddress.ToString() != configuredAddress)
        {
            throw new InvalidOperationException($"Address mismatch! WIF generates {_shopWalletAddress} but config has {configuredAddress}");
        }
    }

    public async Task<CreateCryptoPaymentResponse?> CreatePaymentAsync(CreateCryptoPaymentRequest request, CancellationToken cancellationToken)
    {
        string? symbol = GetBinanceSymbol(request.FiatCurrency);

        if (symbol is null)
        {
            // Couldn't find fiat currency
            return null;
        }

        (bool getPriceSuccess, decimal btcPrice) = await _binanceClient.GetBitcoinPriceAsync(symbol, cancellationToken);

        if (!getPriceSuccess)
        {
            // Failed to get BTC price from Binance
            return null;
        }

        decimal btcAmount = decimal.Round(request.FiatAmount / btcPrice, 8);

        BitcoinAddress paymentAddress = _shopWalletAddress;

        var payment = new CryptoPayment
        {
            Id = Guid.NewGuid(),
            OrderId = request.OrderId,
            FiatAmount = request.FiatAmount,
            FiatCurrency = request.FiatCurrency,
            BitcoinAmount = btcAmount,
            BitcoinAddress = paymentAddress.ToString(),
            Status = CryptoPaymentStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddMinutes(20)
        };

        _db.CryptoPayments.Add(payment);
        await _db.SaveChangesAsync(cancellationToken);

        Console.WriteLine($"Payment {payment.Id} created! Send {btcAmount} BTC to: {paymentAddress}");

        // Payment needs time to process in the blockchain, so for now we can just notify the user that the payment is created
        return new CreateCryptoPaymentResponse(
            payment.Id,
            payment.BitcoinAddress,
            payment.BitcoinAmount,
            payment.ExpiresAt);
    }


    /// <summary>
    /// Checks transaction status on testnet blockchain (Blockstream API)
    /// </summary>
    public async Task<CryptoPaymentStatusResponse?> CheckPaymentStatusAsync(Guid paymentId, bool isSimulation, CancellationToken cancellationToken)
    {
        CryptoPayment? payment = await _db.CryptoPayments.FirstOrDefaultAsync(x => x.Id == paymentId, cancellationToken);

        if (payment is null) return null;

        int confirmations = 0;

        if (isSimulation)
        {
            payment.Status = CryptoPaymentStatus.Confirmed;
        }
        // Check blockchain for transaction
        else if (!string.IsNullOrEmpty(payment.TransactionId))
        {
            BitcoinTransactionDto? tx = await _httpClient.GetFromJsonAsync<BitcoinTransactionDto>(
                $"{_blockstreamApiUrl}/tx/{payment.TransactionId}", cancellationToken);

            if (tx?.Status.Confirmed == true)
            {
                confirmations = 1;
                payment.Status = CryptoPaymentStatus.Confirmed;
            }
        }
        await _db.SaveChangesAsync(cancellationToken);

        // Notify webshop if payment confirmed
        bool webshopNotified = false;
        if (payment.Status == CryptoPaymentStatus.Confirmed)
        {
            webshopNotified = await _webshopClient.SendAsync(paymentId, true);
        }

        return new CryptoPaymentStatusResponse(
            payment.Id,
            payment.Status,
            payment.BitcoinAmount,
            payment.TransactionId,
            confirmations,
            webshopNotified);
    }

    public async Task<GenerateWalletResponse> GenerateShopWalletAsync()
    {
        Key key = new Key();
        BitcoinSecret secret = key.GetBitcoinSecret(Network.TestNet);
        BitcoinAddress address = secret.GetAddress(ScriptPubKeyType.Segwit);

        return await Task.FromResult(new GenerateWalletResponse(secret.ToWif(), address.ToString()));
    }

    public async Task<byte[]> GeneratePaymentQrCodeAsync(Guid paymentId, CancellationToken cancellationToken)
    {
        CryptoPayment? payment = await _db.CryptoPayments.FirstOrDefaultAsync(x => x.Id == paymentId, cancellationToken);

        if (payment is null)
        {
            return new byte[0]; 
        }

        string bitcoinUri = $"bitcoin:{payment.BitcoinAddress}?amount={payment.BitcoinAmount}&label=Order-{payment.OrderId}";

        using var qrGenerator = new QRCodeGenerator();
        using var qrCodeData = qrGenerator.CreateQrCode(bitcoinUri, QRCodeGenerator.ECCLevel.Q);
        using var qrCode = new PngByteQRCode(qrCodeData);

        return qrCode.GetGraphic(20);
    }

    private string? GetBinanceSymbol(Currency currency) => currency switch
    {
        Currency.USD => "BTCUSDT",
        Currency.EUR => "BTCEUR",
        Currency.GBP => "BTCGBP",
        Currency.CHF => "BTCCHF",
        Currency.JPY => "BTCJPY",
        _ => null
    };
}