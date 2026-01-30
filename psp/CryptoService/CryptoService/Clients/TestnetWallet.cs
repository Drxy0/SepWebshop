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
        _blockstreamApiUrl = config["BlockstreamUrl"]
            ?? throw new InvalidOperationException("BlockstreamUrl not configured");
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