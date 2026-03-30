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
            _ = RefreshReportAsync();
        }
    }

    private string Currency
    {
        get { return _currency; }
        set
        {
            _currency = value;
            _ = RefreshReportAsync();
        }
    }

    protected override Task OnInitializedAsync()
    {
        return RefreshReportAsync();
    }

    private async Task RefreshReportAsync()
    {
        _report = await CgtReportCreator.CreateAsync(this.Data, _sellPrice, _currency);
        StateHasChanged();
    }

    private async Task ExportPdf_Click()
    {
        await RefreshReportAsync();

        using var dialog = new FolderBrowserDialog
        {
            Description = "Choose a folder for the report PDF files.",
            ShowNewFolderButton = true
        };

        if (dialog.ShowDialog() != DialogResult.OK)
        {
            return;
        }

        var exportedFiles = ReportPdfExporter.Export(dialog.SelectedPath, _report, _sellPrice, _currency);
        MessageBox.Show(
            $"Exported {exportedFiles.Count} PDF file(s) to {dialog.SelectedPath}.",
            "Report export complete",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information);
    }
}
