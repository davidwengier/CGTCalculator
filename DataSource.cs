namespace CGTCalculator;

internal class DataSource : IDataSource
{
    public Task<IEnumerable<Transaction>> GetTransactionsAsync()
    {
        return Task.FromResult(GetDummyData());
    }

    private IEnumerable<Transaction> GetDummyData()
    {
        yield return new Transaction(new DateOnly(2023, 4, 2), 4, 2.5M);
        yield return new Transaction(new DateOnly(2023, 4, 3), 5, 3.5M);
        yield return new Transaction(new DateOnly(2023, 4, 4), 8, 5.5M);
        yield return new Transaction(new DateOnly(2023, 4, 5), 6, 6.5M);
        yield return new Transaction(new DateOnly(2023, 4, 6), 3, 7.5M);
        yield return new Transaction(new DateOnly(2023, 4, 7), 10, 9.5M);
    }
}
