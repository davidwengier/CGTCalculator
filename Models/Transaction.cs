namespace CGTCalculator;

internal class Transaction
{
    public Guid Id { get; set; }
    public string Symbol { get; set; } = null!;
    public DateOnly Date { get; set; }
    public TransactionType Type { get; set; }
    public decimal Quantity { get; set; }
    public decimal Amount { get; set; }
}
