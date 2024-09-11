using Microsoft.AspNetCore.Components.QuickGrid;

namespace CGTCalculator.Pages;

public partial class Transactions
{
    private QuickGrid<Transaction> _grid = null!;
    private IQueryable<Transaction>? _transactions;

    protected override Task OnInitializedAsync()
    {
        _transactions = this.Data.Transactions;
        return Task.CompletedTask;
    }

    private void Add_Click()
    {
        this.NavigationManager.NavigateTo("/add");
    }

    private void Import_Click()
    {
        this.NavigationManager.NavigateTo("/import");
    }

    private async Task Export_Click()
    {
        using (var dlg = new SaveFileDialog())
        {
            dlg.Filter = "CSV Files|*.csv|All Files|*.*";
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                var transactions = await _transactions!.ToListAsync();
                await Exporter.ExportAsync(dlg.FileName, transactions);
            }
        }
    }

    private void Report_Click()
    {
        this.NavigationManager.NavigateTo("/report");
    }

    private void Edit_Click(Transaction transaction)
    {
        this.NavigationManager.NavigateTo($"/edit/{transaction.Id}");
    }

    private async Task Delete_Click(Transaction transaction)
    {
        // Note to readers: You can't do this in a normal Blazor app :D
        if (MessageBox.Show($"Are you sure to wish to delete this {transaction.Type} of {transaction.Symbol} for {transaction.Value:$#,##0.00}?", "Are you sure?", MessageBoxButtons.YesNo) == DialogResult.Yes)
        {
            this.Data.Remove(transaction);
            await this.Data.SaveChangesAsync();
            //await _grid.RefreshDataAsync().ConfigureAwait(false);
        }
    }

    private string GetTaxYearTitle(int year)
    {
        return $"{year}/{(year + 1) % 100}";
    }
}
