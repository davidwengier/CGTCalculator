using Microsoft.EntityFrameworkCore;

namespace CGTCalculator;

internal static class CgtReportCreator
{
    public static async Task<string[]> CreateAsync(DataSource dataSource, int year)
    {
        var startOfYear = new DateOnly(year, 7, 1);
        var endOfYear = new DateOnly(year + 1, 6, 30);
        var sales = await (from t in dataSource.Transactions
                           where t.Type == TransactionType.Sell
                           where t.Date >= startOfYear
                           where t.Date <= endOfYear
                           select t).ToListAsync();

        return sales.Select(t => t.ToString()).ToArray();
    }
}
