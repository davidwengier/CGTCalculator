namespace CGTCalculator.Pages;

public partial class Report
{
    private CgtReport _report = null!;
    private decimal _sellPrice = 100;
    private string _currency = "USD";

    private bool HasOpen => _report.Open.LineItems.Count > 0;

    private string SellPrice
    {
        get { return _sellPrice.ToString(); }
        set
        {
            decimal.TryParse(value, out _sellPrice);
            _ = OnInitializedAsync();
            StateHasChanged();
        }
    }

    private string Currency
    {
        get { return _currency; }
        set
        {
            _currency = value;
            _ = OnInitializedAsync();
            StateHasChanged();
        }
    }

    protected override async Task OnInitializedAsync()
    {
        _report = await CgtReportCreator.CreateAsync(this.Data, _sellPrice, _currency).ConfigureAwait(false);
    }
}
