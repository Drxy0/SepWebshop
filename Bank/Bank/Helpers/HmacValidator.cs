using System.Security.Cryptography;
using System.Text;

namespace Bank.Helpers;

public static class HmacValidator
{
    public static bool Validate(string payload, string signature, string secretKey)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secretKey));
        byte[] hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
        string computed = Convert.ToHexString(hash).ToLower();

        return computed == signature.ToLower();
    }
}
