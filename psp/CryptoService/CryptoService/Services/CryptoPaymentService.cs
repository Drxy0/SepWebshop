using CryptoService.Clients;
using CryptoService.DTOs;
using CryptoService.Models;
using CryptoService.Persistance;
using Microsoft.EntityFrameworkCore;
using NBitcoin;

namespace CryptoService.Services;

public class CryptoPaymentService : ICryptoPaymentService
{
    private readonly CryptoDbContext _db;
    private readonly HttpClient _httpClient;
    private readonly IBinanceClient _binanceClient;

    private const string BlockstreamTestnetBase = "https://blockstream.info/testnet/api";

    public CryptoPaymentService(CryptoDbContext db, HttpClient httpClient, IBinanceClient binanceClient)
    {
        _db = db;
        _httpClient = httpClient;
        _binanceClient = binanceClient;
    }

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
            "testnet",
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
            var tx = await _httpClient.GetFromJsonAsync<BitcoinTransactionDto>(
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

}
