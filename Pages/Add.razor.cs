using CGTCalculator.Code;
using Microsoft.AspNetCore.Components;

namespace CGTCalculator.Pages;

public partial class Add
{
    [Parameter]
    public Guid Id { get; set; } = Guid.Empty;

    private Transaction _model = new();
    private IReadOnlyList<string> _symbols = Array.Empty<string>();
    private Guid? _loadedTransactionId;

    private bool IsAdd => this.Id == Guid.Empty;

    protected override async Task OnParametersSetAsync()
    {
        if (_loadedTransactionId == this.Id)
        {
            return;
        }

        _loadedTransactionId = this.Id;
        _symbols = await LoadSymbolsAsync();

        if (this.IsAdd)
        {
            _model = CreateAddModel();
        }
        else
        {
            var transaction = await this.DataSource.Transactions
                .FirstOrDefaultAsync(t => t.Id == this.Id);

            if (transaction is null)
            {
                this.NavigationManager.NavigateTo("/transactions");
                return;
            }

            _model = transaction;
        }
    }

    private async Task<IReadOnlyList<string>> LoadSymbolsAsync()
    {
        return await this.DataSource.Transactions
            .AsNoTracking()
            .Select(t => t.Symbol)
            .Distinct()
            .OrderBy(symbol => symbol)
            .ToListAsync();
    }

    private Transaction CreateAddModel()
    {
        var defaults = AddTransactionDefaults.Read();

        return new Transaction
        {
            Symbol = defaults.Symbol.Length > 0
                ? defaults.Symbol
                : _symbols.FirstOrDefault() ?? string.Empty,
            Date = defaults.Date,
            Type = defaults.Type
        };
    }

    private async Task Add_Click()
    {
        _model.Symbol = _model.Symbol.Trim();

        var multiplier = _model.Type == TransactionType.Sell
            ? -1
            : 1;
        _model.Quantity = Math.Abs(_model.Quantity) * multiplier;
        _model.Value = Math.Abs(_model.Value) * multiplier;

        if (this.IsAdd)
        {
            _model.Id = Guid.NewGuid();
            await this.DataSource.Transactions.AddAsync(_model);
            AddTransactionDefaults.Write(_model);
        }

        await this.DataSource.SaveChangesAsync();
        this.NavigationManager.NavigateTo("/transactions");
    }

    private void Cancel_Click()
    {
        this.NavigationManager.NavigateTo("/transactions");
    }
}
