namespace CGTCalculator;

internal interface IDataSource
{
    Task<IEnumerable<Transaction>> GetTransactionsAsync();
}
