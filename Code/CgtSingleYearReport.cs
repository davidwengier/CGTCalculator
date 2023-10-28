namespace CGTCalculator;

public class CgtSingleYearReport
{
    private readonly int _taxYear;

    public int TaxYearSort => _taxYear;
    public string TaxYear => _taxYear < 0 ? "Open" : $"{_taxYear}/{_taxYear + 1 % 100}";

    public List<CgtEvent> LineItems { get; } = new();

    public decimal ShortTermGains => this.LineItems.Where(i => !i.Long).Sum(i => i.Profit);
    public decimal LongTermGains => this.LineItems.Where(i => i.Long).Sum(i => i.Profit);
    public decimal CGTConcession => this.LongTermGains / 2;
    public decimal TotalCapitalGain => this.ShortTermGains + this.LongTermGains - this.CGTConcession;

    public decimal ShortTermTotal => this.LineItems.Where(i => !i.Long).Sum(i => i.SalesValue);
    public decimal LongTermTotal => this.LineItems.Where(i => i.Long).Sum(i => i.SalesValue);
    public decimal TotalValue => this.LineItems.Sum(i => i.SalesValue);

    public CgtSingleYearReport(int taxYear)
    {
        _taxYear = taxYear;
    }
}
