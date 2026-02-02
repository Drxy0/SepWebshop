using CryptoService.Clients.Interfaces;
using CryptoService.DTOs;
using CryptoService.Models;
using CryptoService.Persistance;
using CryptoService.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using NBitcoin;
using QRCoder;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CryptoService.Services;

public class CryptoPaymentService : ICryptoPaymentService
{
    private readonly CryptoDbContext _db;
    private readonly IBinanceClient _binanceClient;
    private readonly IWebshopClient _webshopClient;
    private readonly IHttpClientFactory _httpClientFactory;


    private readonly BitcoinSecret _shopWalletSecret;
    private readonly BitcoinAddress _shopWalletAddress;

    private readonly string _blockstreamApiUrl;

    public CryptoPaymentService(CryptoDbContext db, IHttpClientFactory httpClientFactory, IBinanceClient binanceClient, IConfiguration config, IWebshopClient webshopClient)
    {
        _db = db;
        _binanceClient = binanceClient;
        _webshopClient = webshopClient;
        _httpClientFactory = httpClientFactory;

        _blockstreamApiUrl = config["BlockstreamUrl"] ?? throw new InvalidOperationException("BlockstreamUrl not configured");

        string wif = config["TestShopWallet:Wif"] ?? throw new InvalidOperationException("TestShopWallet:Wif not configured");

        _shopWalletSecret = new BitcoinSecret(wif, Network.TestNet);
        _shopWalletAddress = _shopWalletSecret.GetAddress(ScriptPubKeyType.Segwit);

        string configuredAddress = config["TestShopWallet:Address"] ?? string.Empty;
        if (!string.IsNullOrEmpty(configuredAddress) && _shopWalletAddress.ToString() != configuredAddress)
        {
            throw new InvalidOperationException($"Address mismatch! WIF generates {_shopWalletAddress} but config has {configuredAddress}");
        }
    }

    public async Task<InitializeCryptoPaymentResponse?> CreatePaymentAsync(InitializeCryptoPaymentRequest request, CancellationToken cancellationToken)
    {
        var client = _httpClientFactory.CreateClient("DataServiceClient");
        var response = await client.GetAsync($"d/Payments/{request.MerchantOrderId}", cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception("Payment details not found in DataService.");
        }

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter() }
        };

        var paymentData = await response.Content.ReadFromJsonAsync<DataServicePaymentResponse>(options, cancellationToken);

        if (paymentData == null)
            throw new Exception("Failed to deserialize payment data.");


        string? symbol = GetBinanceSymbol(paymentData.Currency);

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

        decimal btcAmount = decimal.Round((decimal)paymentData.Amount / btcPrice, 8);

        BitcoinAddress paymentAddress = _shopWalletAddress;

        var existingPayment = await _db.CryptoPayments
            .FirstOrDefaultAsync(p => p.MerchantOrderId == request.MerchantOrderId, cancellationToken);

        if (existingPayment is not null)
        {
            existingPayment.FiatAmount = (decimal)paymentData.Amount;
            existingPayment.FiatCurrency = paymentData.Currency;
            existingPayment.BitcoinAmount = btcAmount;
            existingPayment.BitcoinAddress = paymentAddress.ToString();
            existingPayment.Status = CryptoPaymentStatus.Pending;
            existingPayment.CreatedAt = DateTime.UtcNow;
            existingPayment.ExpiresAt = DateTime.UtcNow.AddMinutes(20);

            _db.CryptoPayments.Update(existingPayment);
        }
        else
        {
            var payment = new CryptoPayment
            {
                Id = Guid.NewGuid(),
                MerchantOrderId = request.MerchantOrderId,
                FiatAmount = (decimal)paymentData.Amount,
                FiatCurrency = paymentData.Currency,
                BitcoinAmount = btcAmount,
                BitcoinAddress = paymentAddress.ToString(),
                Status = CryptoPaymentStatus.Pending,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddMinutes(20)
            };

            _db.CryptoPayments.Add(payment);
        }


        try
        {
            await _db.SaveChangesAsync(cancellationToken);
            CryptoPayment savedPayment = existingPayment ?? await _db.CryptoPayments.FirstAsync(p => p.MerchantOrderId == request.MerchantOrderId, cancellationToken);

            Console.WriteLine($"Payment {savedPayment.Id} created/updated! Send {btcAmount} BTC to: {paymentAddress}");

            string qrCodeBase64 = Convert.ToBase64String(await GenerateQrCodeAsync(
                savedPayment.BitcoinAddress,
                savedPayment.BitcoinAmount,
                savedPayment.MerchantOrderId));

            return new InitializeCryptoPaymentResponse(qrCodeBase64, request.MerchantOrderId);
        }
        catch (DbUpdateException ex)
        {
            Console.WriteLine($"Error saving payment to database: {ex.Message}");
            return null;
        }
    }


    /// <summary>
    /// Checks transaction status on testnet blockchain (Blockstream API)
    /// </summary>
    public async Task<CheckPaymentStatusResponse?> CheckPaymentStatusAsync(Guid paymentId, bool isSimulation, CancellationToken cancellationToken)
    {
        CryptoPayment? payment = await _db.CryptoPayments.FirstOrDefaultAsync(x => x.Id == paymentId, cancellationToken);

        if (payment is null)
        {
            return null;
        }

        int confirmations = 0;

        if (isSimulation)
        {
            payment.Status = CryptoPaymentStatus.Confirmed;
        }
        else if (!string.IsNullOrEmpty(payment.TransactionId))
        {
            using var client = _httpClientFactory.CreateClient();

            BitcoinTransactionDto? tx = await client.GetFromJsonAsync<BitcoinTransactionDto>(
                $"{_blockstreamApiUrl}/tx/{payment.TransactionId}", cancellationToken);

            if (tx?.Status.Confirmed == true)
            {
                confirmations = 1;
                payment.Status = CryptoPaymentStatus.Confirmed;
            }
        }

        await _db.SaveChangesAsync(cancellationToken);

        bool webshopNotified = false;

        if (payment.Status == CryptoPaymentStatus.Confirmed)
        {
            webshopNotified = await _webshopClient.SendAsync(paymentId, true);
        }

        return new CheckPaymentStatusResponse(
            payment.Id,
            payment.Status,
            payment.BitcoinAmount,
            payment.TransactionId,
            confirmations,
            webshopNotified);
    }

    private async Task<byte[]> GenerateQrCodeAsync(string bitcoinAddress, decimal bitcoinAmount, Guid merchantOrderId)
    {
        string bitcoinUri = $"bitcoin:{bitcoinAddress}?amount={bitcoinAmount}&label=Order-{merchantOrderId}";

        using var qrGenerator = new QRCodeGenerator();
        using var qrCodeData = qrGenerator.CreateQrCode(bitcoinUri, QRCodeGenerator.ECCLevel.Q);
        using var qrCode = new PngByteQRCode(qrCodeData);

        return qrCode.GetGraphic(20);
    }

    public async Task<GenerateWalletResponse> GenerateShopWalletAsync()
    {
        Key key = new Key();
        BitcoinSecret secret = key.GetBitcoinSecret(Network.TestNet);
        BitcoinAddress address = secret.GetAddress(ScriptPubKeyType.Segwit);

        return await Task.FromResult(new GenerateWalletResponse(secret.ToWif(), address.ToString()));
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