namespace CGTCalculator;

internal static class CgtReportCreator
{
    public static async Task<List<CgtReport>> CreateAsync(DataSource dataSource, int yearToReport)
    {
        var startOfYear = new DateOnly(yearToReport, 7, 1);
        var endOfYear = new DateOnly(yearToReport + 1, 6, 30);
        var transactions = await dataSource.Transactions.AsNoTracking().OrderBy(t => t.Date).ToListAsync();

        var openTransactions = new List<Transaction>();
        var results = new Dictionary<int, CgtReport>();
        foreach (var t in transactions)
        {
            var report = GetCgtReport(results, t);

            if (t.Type == TransactionType.Buy)
            {
                openTransactions.Add(t);
            }
            else
            {
                var buys = FillSale(openTransactions, t);
                report.AddSale(t, buys);
            }
        }

        return results.Values.OrderBy(r => r.TaxYearSort).ToList();
    }

    private static List<Transaction> FillSale(List<Transaction> openTransactions, Transaction saleTransaction)
    {
        var quantityToSell = Math.Abs(saleTransaction.Quantity);
        var soldTransactions = new List<Transaction>();
        for (var i = 0; i < openTransactions.Count; i++)
        {
            var t = openTransactions[i];
            if (t.Symbol != saleTransaction.Symbol)
            {
                continue;
            }

            if (quantityToSell == 0)
            {
                break;
            }

            if (t.Quantity > quantityToSell)
            {
                var originalQuantity = t.Quantity;
                var originalSharePrice = t.Value / t.Quantity;
                var newTransaction = new Transaction
                {
                    Quantity = quantityToSell,
                    Symbol = t.Symbol,
                    Date = t.Date,
                    Type = t.Type,
                    Value = originalSharePrice * quantityToSell,
                    Id = Guid.Empty
                };

                // We're using NoTracking, so these changes won't make it into the database
                t.Value = originalSharePrice * (originalQuantity - quantityToSell);
                t.Quantity -= quantityToSell;
                soldTransactions.Add(newTransaction);
                break;
            }

            soldTransactions.Add(t);
            quantityToSell -= t.Quantity;
            openTransactions.RemoveAt(i);
            i--;
        }

        return soldTransactions;
    }

    private static CgtReport GetCgtReport(Dictionary<int, CgtReport> results, Transaction t)
    {
        var taxYear = t.Date.Month >= 7
            ? t.Date.Year
            : t.Date.Year - 1;

        if (!results.TryGetValue(taxYear, out var report))
        {
            report = new CgtReport(taxYear);
            results.Add(taxYear, report);
        }

        return report;
    }
}
