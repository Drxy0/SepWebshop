using CryptoService.Clients.Interfaces;
using NBitcoin;

namespace CryptoService.Clients;

public class TestnetWallet : ITestnetWallet
{
    private readonly HttpClient _httpClient;

    public TestnetWallet(HttpClient httpClient)
    {
        _httpClient = httpClient;
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
    }

    /// <summary>
    /// Generates a new Testnet wallet and returns the WIF (private key) and address.
    /// </summary>
    public (string Wif, string Address) GenerateWifWallet()
    {
        // Create a new random key
        Key key = new Key();

        // Get the BitcoinSecret (WIF) for Testnet
        BitcoinSecret secret = key.GetBitcoinSecret(Network.TestNet);

        // Get the SegWit address
        BitcoinAddress address = secret.GetAddress(ScriptPubKeyType.Segwit);

        // Log info
        Console.WriteLine($"[Wallet] Generated new Testnet wallet:");
        Console.WriteLine($"[Wallet] Address: {address}");
        Console.WriteLine($"[Wallet] WIF: {secret}");

        // Return as strings
        return (secret.ToWif(), address.ToString());
    }

=======
>>>>>>> 69563e2 (Add wallet, start)
}

// Helper classes for deserialization
public class BlockstreamUtxo
{
    public string txid { get; set; }
    public int vout { get; set; }
    public long value { get; set; } // in satoshis
}

public class BlockstreamTx
{
    public BlockstreamTxStatus status { get; set; }
}

public class BlockstreamTxStatus
{
    public bool confirmed { get; set; }
    public int? block_height { get; set; }
}
