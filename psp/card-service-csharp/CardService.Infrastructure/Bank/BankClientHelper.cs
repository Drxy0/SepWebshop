using CardService.Domain.Enums;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace CardService.Infrastructure.Bank
{
    internal static class BankClientHelper
    {
        internal static string BuildPayload(string merchantId, double amount, Currency currency, string stan, DateTime timestamp)
        {
            return
                $"merchantId={merchantId}" +
                $"&amount={amount.ToString(CultureInfo.InvariantCulture)}" +
                $"&currency={(int)currency}" +
                $"&stan={stan}" +
                $"&timestamp={timestamp:O}";
        }

        internal static string GenerateHmac(string payload, string secretKey)
        {
            using var hmac = new HMACSHA256(
                Encoding.UTF8.GetBytes(secretKey));

            byte[] hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));

            return Convert.ToHexString(hash).ToLowerInvariant();
        }

        internal static string GenerateStan() => Random.Shared.Next(100000, 999999).ToString();
    }
}
