using NBitcoin;

namespace CryptoService.Clients.Interfaces
{
    public interface ITestnetWallet
    {
        /// Generates a new testnet wallet (private key + address)
        (BitcoinSecret Secret, BitcoinAddress Address) GenerateWallet();

        /// Sends BTC from the provided wallet to a target testnet address
        Task<string> SendPaymentAsync(BitcoinSecret senderSecret, string destinationAddress, decimal amountBtc, CancellationToken cancellationToken = default);

        /// Checks if a transaction on testnet has been confirmed
        Task<bool> IsConfirmedAsync(string txId, CancellationToken cancellationToken = default);

        (string Wif, string Address) GenerateWifWallet();
    }
}
