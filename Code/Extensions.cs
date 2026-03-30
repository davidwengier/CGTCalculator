namespace CGTCalculator;

internal static class Extensions
{
    public static DateOnly ToDateOnly(this DateTime dateTime)
    {
        return new DateOnly(dateTime.Year, dateTime.Month, dateTime.Day);
    }

    public static string ToTaxYearTitle(this int taxYear)
    {
        var nextYear = taxYear + 1;
        return nextYear % 100 == 0
            ? $"{taxYear}/{nextYear}"
            : $"{taxYear}/{nextYear % 100:00}";
    }
}
