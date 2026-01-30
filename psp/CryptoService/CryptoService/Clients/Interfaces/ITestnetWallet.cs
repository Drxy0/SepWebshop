using NBitcoin;

namespace CryptoService.Clients.Interfaces
{
    public interface IWalletHelper
    {
        Task<bool> IsConfirmedAsync(string txId, CancellationToken cancellationToken = default);
    }
}
