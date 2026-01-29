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

        if (!response.IsSuccessStatusCode)
            throw new Exception("Failed to broadcast transaction: " + await response.Content.ReadAsStringAsync());

        return tx.GetHash().ToString();
    }


    // 3. Check transaction status
    public async Task<bool> IsConfirmedAsync(string txId, CancellationToken cancellationToken = default)
    {
        BlockstreamTx? tx = await _httpClient.GetFromJsonAsync<BlockstreamTx>(
            $"https://blockstream.info/testnet/api/tx/{txId}", cancellationToken);

        return tx?.status?.confirmed ?? false;
    }
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
