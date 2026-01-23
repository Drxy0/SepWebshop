using Bank.Contracts;
using System.Globalization;

namespace Bank.Helpers;

public static class QRIpsPayloadGenerator
{
    public static string Generate(QRIpsData data)
    {
        string amount = data.Amount
            .ToString("0.00", CultureInfo.InvariantCulture)
            .Replace(".", ",");

        string payload =
            $"K:PR|" +
            $"V:01|" +
            $"C:1|" +
            $"R:{data.MerchantAccount}|" +
            $"N:{data.MerchantName}|" +
            $"I:RSD{amount}|" +
            $"SF:289|" +
            $"S:Placanje robe";


        return payload;
    }
}
