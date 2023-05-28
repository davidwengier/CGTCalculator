namespace CGTCalculator;

internal class CgtReport(int taxYear)
{
    private readonly List<(Transaction, List<Transaction>)> _sales = new();

    public int TaxYearSort => taxYear;
    public string TaxYear => $"{taxYear}/{taxYear + 1 % 100}";
    public List<(Transaction Sale, List<Transaction> Buys)> Sales => _sales;

    public void AddSale(Transaction sale, List<Transaction> buys)
    {
        _sales.Add((sale, buys));
    }
}
