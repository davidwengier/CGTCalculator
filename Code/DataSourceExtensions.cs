namespace CGTCalculator;

internal static class DataSourceExtensions
{
    public static List<string> GetAllSymbols(this DataSource dataSource)
    {
        return dataSource.Transactions.Select(x => x.Symbol).Distinct().ToList();
    }
}
