<<<<<<< HEAD
<<<<<<< HEAD
﻿using CryptoService.Clients.Interfaces;
using CryptoService.DTOs;
using CryptoService.Models;
using CryptoService.Persistance;
using CryptoService.Services.Interfaces;
=======
﻿using CryptoService.Clients;
using CryptoService.DTOs;
using CryptoService.Models;
using CryptoService.Persistance;
>>>>>>> 5cbd7fe (Add base implementation)
=======
﻿using CryptoService.Clients.Interfaces;
using CryptoService.DTOs;
using CryptoService.Models;
using CryptoService.Persistance;
using CryptoService.Services.Interfaces;
>>>>>>> 69563e2 (Add wallet, start)
using Microsoft.EntityFrameworkCore;
using NBitcoin;

namespace CryptoService.Services;

public class CryptoPaymentService : ICryptoPaymentService
{
    private readonly CryptoDbContext _db;
    private readonly HttpClient _httpClient;
    private readonly IBinanceClient _binanceClient;
<<<<<<< HEAD
<<<<<<< HEAD
<<<<<<< HEAD
    private readonly ITestnetWallet _testnetWallet;
=======
    private readonly IWalletHelper _walletHelper;
>>>>>>> 5ab45fd (Finish crypto backend)

    private readonly BitcoinSecret _shopWalletSecret;
    private readonly BitcoinAddress _shopWalletAddress;

<<<<<<< HEAD
    public CryptoPaymentService(CryptoDbContext db, HttpClient httpClient, IBinanceClient binanceClient, ITestnetWallet testnetWallet, IConfiguration config)
=======

    private const string BlockstreamTestnetBase = "https://blockstream.info/testnet/api";

    public CryptoPaymentService(CryptoDbContext db, HttpClient httpClient, IBinanceClient binanceClient)
>>>>>>> 5cbd7fe (Add base implementation)
=======
    private readonly ITestnetWallet _testnetWallet;

    private const string BlockstreamTestnetBase = "https://blockstream.info/testnet/api";

    public CryptoPaymentService(CryptoDbContext db, HttpClient httpClient, IBinanceClient binanceClient, ITestnetWallet testnetWallet)
>>>>>>> 69563e2 (Add wallet, start)
=======
    private readonly string _blockstreamApiUrl;

    public CryptoPaymentService(
        CryptoDbContext db,
        HttpClient httpClient,
        IBinanceClient binanceClient,
        IWalletHelper walletHelper,
        IConfiguration config)
>>>>>>> 5ab45fd (Finish crypto backend)
    {
        _db = db;
        _httpClient = httpClient;
        _binanceClient = binanceClient;
<<<<<<< HEAD
<<<<<<< HEAD
<<<<<<< HEAD
        _testnetWallet = testnetWallet;
        _shopWalletSecret = new BitcoinSecret(config["BitcoinTestnetWalletWif"], Network.TestNet); // TODO: Make method that generates a wallet and a secret, save both
        _shopWalletAddress = _shopWalletSecret.GetAddress(ScriptPubKeyType.Segwit);
=======
        _testnetWallet = testnetWallet;
>>>>>>> 69563e2 (Add wallet, start)
=======
        _walletHelper = walletHelper;

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

>>>>>>> 5ab45fd (Finish crypto backend)
    }

    /// <summary>
    /// Generate a payment record with shop's main address for customer to pay
    /// </summary>
<<<<<<< HEAD
=======
    }

>>>>>>> 5cbd7fe (Add base implementation)
=======
>>>>>>> 69563e2 (Add wallet, start)
    public async Task<CreateCryptoPaymentResponse> CreatePaymentAsync(CreateCryptoPaymentRequest request, CancellationToken cancellationToken)
    {
        // 1. Get real-time BTC price
        string symbol = GetBinanceSymbol(request.FiatCurrency);
        decimal btcPrice = await _binanceClient.GetBitcoinPriceAsync(symbol, cancellationToken);
        decimal btcAmount = decimal.Round(request.FiatAmount / btcPrice, 8);

        // 2. Use shop's main address for all payments
        BitcoinAddress paymentAddress = _shopWalletAddress;

        // 3. Store payment
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

        return new CreateCryptoPaymentResponse(
            payment.Id,
            payment.BitcoinAddress,
            payment.BitcoinAmount,
<<<<<<< HEAD
=======
            "testnet",
>>>>>>> 5cbd7fe (Add base implementation)
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
<<<<<<< HEAD
<<<<<<< HEAD
            BitcoinTransactionDto? tx = await _httpClient.GetFromJsonAsync<BitcoinTransactionDto>(
<<<<<<< HEAD
=======
            var tx = await _httpClient.GetFromJsonAsync<BitcoinTransactionDto>(
>>>>>>> 5cbd7fe (Add base implementation)
=======
            BitcoinTransactionDto? tx = await _httpClient.GetFromJsonAsync<BitcoinTransactionDto>(
>>>>>>> 69563e2 (Add wallet, start)
                    $"{BlockstreamTestnetBase}/tx/{payment.TransactionId}",
                    cancellationToken);
=======
                $"{_blockstreamApiUrl}/tx/{payment.TransactionId}", cancellationToken);
>>>>>>> 5ab45fd (Finish crypto backend)

            if (tx?.Status.Confirmed == true && tx.Status.BlockHeight.HasValue)
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

<<<<<<< HEAD
<<<<<<< HEAD
=======
>>>>>>> 69563e2 (Add wallet, start)
    /// <summary>
<<<<<<< HEAD
    /// Simulate sending BTC, generates a mock transaction ID and marks payment as confirmed
    /// </summary>

    public async Task<string> ProcessPaymentAsync(Guid paymentId, CancellationToken cancellationToken)
    {
        var payment = await _db.CryptoPayments.FirstOrDefaultAsync(x => x.Id == paymentId, cancellationToken);
        if (payment is null) throw new Exception("Payment not found");
        if (payment.Status != CryptoPaymentStatus.Pending)
            throw new InvalidOperationException("Payment is already processed");

<<<<<<< HEAD
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
=======
        // 1. Generate a temporary student testnet wallet
        (BitcoinSecret? secret, BitcoinAddress? studentAddress) = _testnetWallet.GenerateWallet();
        Console.WriteLine($"Fund this testnet address using a faucet: {studentAddress}");

        // 2. Wait until testnet wallet has funds (manual step) or simulate delay in demo

        // 3. Send BTC to the shop address
        string txId = await _testnetWallet.SendPaymentAsync(secret, payment.BitcoinAddress, payment.BitcoinAmount);

        // 4. Store transaction ID
        payment.TransactionId = txId;
        payment.Status = CryptoPaymentStatus.Confirmed; // optionally mark immediately

        await _db.SaveChangesAsync(cancellationToken);

>>>>>>> 69563e2 (Add wallet, start)
        return txId;
    }


    /// <summary>
    /// Checks transaction status on testnet API (Blockstream)
=======
    /// Checks transaction status on testnet blockchain (Blockstream API)
>>>>>>> 5ab45fd (Finish crypto backend)
    /// </summary>
    public async Task<CryptoPaymentStatusResponse?> CheckPaymentStatusAsync(Guid paymentId, CancellationToken cancellationToken)
    {
        CryptoPayment? payment = await _db.CryptoPayments
            .FirstOrDefaultAsync(x => x.Id == paymentId, cancellationToken);

        if (payment is null) return null;

        int confirmations = 0;

        if (!string.IsNullOrEmpty(payment.TransactionId))
        {
            try
            {
                bool confirmed = await _walletHelper.IsConfirmedAsync(payment.TransactionId, cancellationToken);

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

<<<<<<< HEAD
<<<<<<< HEAD
    public async Task<GenerateWalletResponse> GenerateWalletAsync()
=======
    public async Task<GenerateWalletResponse> GenerateShopWalletAsync()
>>>>>>> 5ab45fd (Finish crypto backend)
    {
        Key key = new Key();
        BitcoinSecret secret = key.GetBitcoinSecret(Network.TestNet);
        BitcoinAddress address = secret.GetAddress(ScriptPubKeyType.Segwit);

        return await Task.FromResult(new GenerateWalletResponse(secret.ToWif(), address.ToString()));
    }
<<<<<<< HEAD
=======
>>>>>>> 5cbd7fe (Add base implementation)
=======
>>>>>>> 69563e2 (Add wallet, start)
}
=======

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
>>>>>>> 5ab45fd (Finish crypto backend)
