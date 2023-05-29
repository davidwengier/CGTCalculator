namespace CGTCalculator;

internal static class CgtReportCreator
{
    public static async Task<List<CgtReport>> CreateAsync(DataSource dataSource)
    {
        var transactions = await dataSource.Transactions.AsNoTracking().OrderBy(t => t.Date).ToListAsync();

        var openTransactions = new List<Transaction>();
        var results = new Dictionary<int, CgtReport>();
        foreach (var t in transactions)
        {
            if (t.Type == TransactionType.Buy)
            {
                openTransactions.Add(t);
            }
            else
            {
                var report = GetCgtReport(results, t);
                report.LineItems.AddRange(CreateLineItems(openTransactions, t));
            }
        }

        return results.Values.OrderBy(r => r.TaxYearSort).ToList();
    }

    private static List<CgtEvent> CreateLineItems(List<Transaction> openTransactions, Transaction saleTransaction)
    {
        var lineItems = new List<CgtEvent>();
        var quantityToSell = Math.Abs(saleTransaction.Quantity);
        var saleSharePrice = saleTransaction.Value / saleTransaction.Quantity;
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

                // We're using NoTracking, so these changes won't make it into the database, but we need them for finding
                // parts of future sales
                t.Value = originalSharePrice * (originalQuantity - quantityToSell);
                t.Quantity -= quantityToSell;

                lineItems.Add(new CgtEvent
                {
                    Symbol = saleTransaction.Symbol,
                    PurchaseDate = t.Date,
                    SellDate = saleTransaction.Date,
                    Quantity = quantityToSell,
                    CostBase = originalSharePrice * quantityToSell,
                    SalesValue = saleSharePrice * quantityToSell,
                });

                break;
            }

            lineItems.Add(new CgtEvent
            {
                Symbol = saleTransaction.Symbol,
                PurchaseDate = t.Date,
                SellDate = saleTransaction.Date,
                Quantity = t.Quantity,
                CostBase = t.Value,
                SalesValue = saleSharePrice * t.Quantity,
            });

            quantityToSell -= t.Quantity;
            openTransactions.RemoveAt(i);
            i--;
        }

        return lineItems;
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
