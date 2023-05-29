namespace CGTCalculator;

internal class CgtEvent
{
    public required string Symbol { get; init; }
    public required DateOnly PurchaseDate { get; init; }
    public required DateOnly SellDate { get; init; }
    public required decimal Quantity { get; init; }
    public required decimal CostBase { get; init; }
    public required decimal SalesValue { get; init; }

    public decimal Profit => this.SalesValue - this.CostBase;
    public bool Long => this.SellDate.DayNumber - this.PurchaseDate.DayNumber > 365;
}
