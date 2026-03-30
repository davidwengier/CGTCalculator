namespace CGTCalculator;

public class CgtSingleYearReport
{
    private readonly int _taxYear;

    public int TaxYearSort => _taxYear;
    public string TaxYear => _taxYear.ToTaxYearTitle();

    public List<CgtEvent> LineItems { get; } = new();
    internal List<Transaction> ContributingTransactions { get; } = new();

    public decimal ShortTermGains => this.LineItems.Where(i => !i.Long).Sum(i => i.Profit);
    public decimal LongTermGains => this.LineItems.Where(i => i.Long).Sum(i => i.Profit);
    public decimal CGTConcession => this.LongTermGains / 2;
    public decimal TotalCapitalGain => this.ShortTermGains + this.LongTermGains - this.CGTConcession;

    public decimal ShortTermTotal => this.LineItems.Where(i => !i.Long).Sum(i => i.SalesValue);
    public decimal LongTermTotal => this.LineItems.Where(i => i.Long).Sum(i => i.SalesValue);
    public decimal TotalValue => this.LineItems.Sum(i => i.SalesValue);

    public decimal TotalBuysAndWashes { get; set; }

    public CgtSingleYearReport(int taxYear)
    {
        _taxYear = taxYear;
    }

    internal void AddContributingTransaction(Transaction transaction)
    {
        if (this.ContributingTransactions.Any(existing => existing.Id == transaction.Id))
        {
            return;
        }

        this.ContributingTransactions.Add(transaction);
    }
}
