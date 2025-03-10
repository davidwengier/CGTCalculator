using Microsoft.AspNetCore.Components;

namespace CGTCalculator.Pages;

public partial class Add
{
    [Parameter]
    public Guid Id { get; set; } = Guid.Empty;

    private Transaction _model = new()
    {
        Date = DateTime.Now.ToDateOnly()
    };

    private bool IsAdd => this.Id == Guid.Empty;

    protected override async Task OnInitializedAsync()
    {
        if (!this.IsAdd)
        {
            _model = await this.DataSource.Transactions.Where(t => t.Id == this.Id).FirstAsync().ConfigureAwait(false);
        }
    }

    protected override void OnParametersSet()
    {
        if (this.IsAdd)
        {
            _model.Symbol = this.DataSource.Transactions.OrderByDescending(t=>t.Date).FirstOrDefault()?.Symbol ?? string.Empty;
        }
    }

    private async Task Add_Click()
    {
        var multiplier = _model.Type == TransactionType.Sell
            ? -1
            : 1;
        _model.Quantity = Math.Abs(_model.Quantity) * multiplier;
        _model.Value = Math.Abs(_model.Value) * multiplier;

        if (this.IsAdd)
        {
            _model.Id = Guid.NewGuid();
            await this.DataSource.Transactions.AddAsync(_model).ConfigureAwait(false);
        }

        await this.DataSource.SaveChangesAsync().ConfigureAwait(false);
        this.NavigationManager.NavigateTo("/transactions");
    }

    private void Cancel_Click()
    {
        this.NavigationManager.NavigateTo("/transactions");
    }
}
