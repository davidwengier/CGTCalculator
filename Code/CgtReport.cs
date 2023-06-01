namespace CGTCalculator;

internal class CgtReport
{
    private readonly int _taxYear;
    private readonly List<(Transaction, List<Transaction>)> _sales = new();

    public int TaxYearSort => _taxYear;
    public string TaxYear => $"{_taxYear}/{_taxYear + 1 % 100}";
    public List<(Transaction Sale, List<Transaction> Buys)> Sales => _sales;

    public List<CgtEvent> LineItems { get; } = new();

    public CgtReport(int taxYear)
    {
        _taxYear = taxYear;
    }
}
