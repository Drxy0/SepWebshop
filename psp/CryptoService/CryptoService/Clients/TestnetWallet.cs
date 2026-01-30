using CryptoService.Clients.Interfaces;
using Microsoft.Extensions.Configuration;
using NBitcoin;

namespace CryptoService.Clients;

public class WalletHelper : IWalletHelper
{
    private readonly HttpClient _httpClient;
    private readonly string _blockstreamApiUrl;

    public WalletHelper(HttpClient httpClient, IConfiguration config)
    {
        _httpClient = httpClient;
<<<<<<< HEAD
    }

    // 1. Generate wallet
    public (BitcoinSecret Secret, BitcoinAddress Address) GenerateWallet()
    {
        Key key = new Key();
        BitcoinSecret secret = key.GetBitcoinSecret(Network.TestNet);
        BitcoinAddress address = secret.GetAddress(ScriptPubKeyType.Segwit);
        return (secret, address);
    }

    // 2. Send BTC to shop address
    public async Task<string> SendPaymentAsync(BitcoinSecret senderSecret, string destinationAddress, decimal amountBtc, CancellationToken cancellationToken = default)
    {
        BitcoinAddress senderAddress = senderSecret.GetAddress(ScriptPubKeyType.Segwit);

<<<<<<< HEAD
        // 1. Get UTXOs for the sender
        List<BlockstreamUtxo>? utxos = await _httpClient.GetFromJsonAsync<List<BlockstreamUtxo>>(
            $"https://blockstream.info/testnet/api/address/{senderAddress}/utxo", cancellationToken);

        if (utxos == null || !utxos.Any())
            throw new Exception($"Wallet {senderAddress} has no funds. Fund it via a Testnet faucet.");

        // 2. Prepare coins for transaction
        Coin[] coins = utxos.Select(u =>
            new Coin(uint256.Parse(u.txid), (uint)u.vout, Money.Satoshis(u.value), senderAddress.ScriptPubKey)
        ).ToArray();

        // 3. Create transaction builder
        TransactionBuilder txBuilder = Network.TestNet.CreateTransactionBuilder();
        txBuilder.AddCoins(coins);
        txBuilder.AddKeys(senderSecret);

        // 4. Send BTC to destination
        txBuilder.Send(BitcoinAddress.Create(destinationAddress, Network.TestNet), Money.Coins(amountBtc));

        // 5. Set change back to sender
        txBuilder.SetChange(senderAddress);

        // 6. Let NBitcoin estimate fees automatically
        var tx = txBuilder.BuildTransaction(true);

        // 7. Verify transaction is fully funded
        if (!txBuilder.Verify(tx))
            throw new Exception("Transaction could not be built: insufficient funds for amount + fee.");

        // 8. Broadcast transaction
        var rawTx = tx.ToHex();
        var response = await _httpClient.PostAsync(
            "https://blockstream.info/testnet/api/tx",
            new StringContent(rawTx),
            cancellationToken
        );
=======
        // Get UTXOs
        List<BlockstreamUtxo>? utxos = await _httpClient.GetFromJsonAsync<List<BlockstreamUtxo>>(
            $"https://blockstream.info/testnet/api/address/{senderAddress}/utxo",
            cancellationToken);

        if (utxos == null || !utxos.Any())
            throw new Exception("No funds in wallet. Fund using a testnet faucet.");

        TransactionBuilder txBuilder = Network.TestNet.CreateTransactionBuilder();

        Coin[] coins = utxos.Select(u => new Coin(
            uint256.Parse(u.txid),
            (uint)u.vout,
            Money.Satoshis(u.value),
            senderAddress.ScriptPubKey
        )).ToArray();

        txBuilder.AddCoins(coins);
        txBuilder.AddKeys(senderSecret);
        txBuilder.Send(BitcoinAddress.Create(destinationAddress, Network.TestNet), Money.Coins(amountBtc));
        txBuilder.SendFees(Money.Satoshis(1000)); // small fee
        txBuilder.SetChange(senderAddress);

        var tx = txBuilder.BuildTransaction(true);

        // Broadcast
        var rawTx = tx.ToHex();
        var response = await _httpClient.PostAsync("https://blockstream.info/testnet/api/tx", new StringContent(rawTx), cancellationToken);
>>>>>>> 69563e2 (Add wallet, start)

        if (!response.IsSuccessStatusCode)
            throw new Exception("Failed to broadcast transaction: " + await response.Content.ReadAsStringAsync());

        return tx.GetHash().ToString();
    }


<<<<<<< HEAD

=======
>>>>>>> 69563e2 (Add wallet, start)
    // 3. Check transaction status
    public async Task<bool> IsConfirmedAsync(string txId, CancellationToken cancellationToken = default)
    {
        BlockstreamTx? tx = await _httpClient.GetFromJsonAsync<BlockstreamTx>(
            $"https://blockstream.info/testnet/api/tx/{txId}", cancellationToken);

        return tx?.status?.confirmed ?? false;
    }
<<<<<<< HEAD

    public async Task<decimal> GetBalanceAsync(BitcoinAddress address, CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"[Wallet] Checking balance for {address}");

        var utxos = await _httpClient.GetFromJsonAsync<List<BlockstreamUtxo>>(
            $"https://blockstream.info/testnet/api/address/{address}/utxo", cancellationToken);

        long totalSats = utxos?.Sum(x => x.value) ?? 0;

        decimal btc = totalSats / 100_000_000m;

        Console.WriteLine($"[Wallet] Balance: {btc} BTC");

        return btc;
=======
        _blockstreamApiUrl = config["BlockstreamUrl"]
            ?? throw new InvalidOperationException("BlockstreamUrl not configured");
>>>>>>> 5ab45fd (Finish crypto backend)
    }

    /// <summary>
    /// Check if a transaction is confirmed on the blockchain
    /// </summary>
    public async Task<bool> IsConfirmedAsync(string txId, CancellationToken cancellationToken = default)
    {
        try
        {
            BlockstreamTx? tx = await _httpClient.GetFromJsonAsync<BlockstreamTx>(
                $"{_blockstreamApiUrl}/tx/{txId}", cancellationToken);

            return tx?.status?.confirmed ?? false;
        }
        catch
        {
            return false; // Transaction not found yet
        }
    }
<<<<<<< HEAD

=======
>>>>>>> 69563e2 (Add wallet, start)
=======
>>>>>>> 5ab45fd (Finish crypto backend)
}

// Helper classes for deserialization
public class BlockstreamTx
{
    public BlockstreamTxStatus status { get; set; } = new();
}

public class BlockstreamTxStatus
{
    public bool confirmed { get; set; }
    public int? block_height { get; set; }
}