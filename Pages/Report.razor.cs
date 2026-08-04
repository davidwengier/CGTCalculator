using System.Text.Json;

namespace CGTCalculator.Pages;

public partial class Report
{
    private readonly SemaphoreSlim _refreshLock = new(1, 1);
    private CgtReport? _report;
    private string? _reportError;
    private decimal _sellPrice = 100;
    private string _currency = "USD";
    private int _refreshVersion;

    private bool HasOpen => _report?.Open.LineItems.Count > 0;

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
        var refreshVersion = Interlocked.Increment(ref _refreshVersion);
        await _refreshLock.WaitAsync();
        try
        {
            if (refreshVersion != _refreshVersion)
            {
                return;
            }

            _report = await CgtReportCreator.CreateAsync(this.Data, _sellPrice, _currency);
            _reportError = null;
        }
        catch (Exception ex) when (ex is HttpRequestException or JsonException or InvalidDataException or KeyNotFoundException)
        {
            _report = null;
            _reportError = $"Unable to load exchange rates: {ex.Message}";
        }
        finally
        {
            _refreshLock.Release();
        }

        await InvokeAsync(StateHasChanged);
    }

    private async Task ExportPdf_Click()
    {
        await RefreshReportAsync();

        if (_report is null)
        {
            return;
        }

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
