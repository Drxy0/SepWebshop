using System.Globalization;

namespace Bank.Helpers;

public static class DebitCardHelper
{
    public static bool IsCardExpired(string expirationDate)
    {
        if (!DateTime.TryParseExact(
            expirationDate,
            "MM/yy",
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out DateTime expiryDate))
        {
            return true;
        }

        DateTime lastDayOfMonth = new DateTime(
            expiryDate.Year,
            expiryDate.Month,
            DateTime.DaysInMonth(expiryDate.Year, expiryDate.Month)
        );

        return lastDayOfMonth < DateTime.UtcNow.Date;
    }
}
