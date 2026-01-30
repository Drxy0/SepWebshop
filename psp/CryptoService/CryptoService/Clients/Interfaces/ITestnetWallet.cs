using NBitcoin;

namespace CryptoService.Clients.Interfaces
{
    public interface IWalletHelper
    {
        Task<bool> IsConfirmedAsync(string txId, CancellationToken cancellationToken = default);
<<<<<<< HEAD
<<<<<<< HEAD

        (string Wif, string Address) GenerateWifWallet();
=======
>>>>>>> 69563e2 (Add wallet, start)
=======
>>>>>>> 5ab45fd (Finish crypto backend)
    }
}
