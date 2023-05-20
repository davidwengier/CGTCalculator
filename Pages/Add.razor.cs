using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;

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

    private async Task Add_Click()
    {
        var multiplier = _model.Type == TransactionType.Buy
            ? 1
            : -1;
        _model.Quantity = Math.Abs(_model.Quantity) * multiplier;
        _model.Value = Math.Abs(_model.Value) * multiplier;

        if (this.IsAdd)
        {
            _model.Id = Guid.NewGuid();
            await this.DataSource.Transactions.AddAsync(_model).ConfigureAwait(false);
        }

        await this.DataSource.SaveChangesAsync().ConfigureAwait(false);
        this.NavigationManager.NavigateTo("/");
    }

    private void Cancel_Click()
    {
        this.NavigationManager.NavigateTo("/transactions");
    }
}
