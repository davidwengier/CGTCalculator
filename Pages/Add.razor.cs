namespace CGTCalculator.Pages;

public partial class Add
{
    private readonly TransactionModel _model = new();

    private async Task Add_Click()
    {
        var multiplier = _model.Type == TransactionType.Buy
            ? 1
            : -1;
        var transaction = new Transaction
        {
            Id = Guid.NewGuid(),
            Symbol = _model.Symbol,
            Date = _model.Date,
            Type = _model.Type,
            Quantity = Math.Abs(_model.Quantity) * multiplier,
            Value = Math.Abs(_model.Value) * multiplier,
        };

        await this.DataSource.Transactions.AddAsync(transaction).ConfigureAwait(false);
        await this.DataSource.SaveChangesAsync().ConfigureAwait(false);
        this.NavigationManager.NavigateTo("/");
    }

    private void Cancel_Click()
    {
        this.NavigationManager.NavigateTo("/transactions");
    }

    private class TransactionModel
    {
        public string Symbol = "";
        public DateOnly Date = DateTime.Now.ToDateOnly();
        public TransactionType Type;
        public decimal Quantity;
        public decimal Value;
    }
}
