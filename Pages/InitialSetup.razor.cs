using CGTCalculator.Code;

namespace CGTCalculator.Pages;

public partial class InitialSetup
{
    private const string DatabaseFileDialogFilter = "DB Files|*.db|All Files|*.*";
    private const string DatabaseFileKeyName = "DatabaseFile";

    protected override Task OnInitializedAsync()
    {
        if (SettingsStorage.ReadString(DatabaseFileKeyName) is { Length: > 0 } databaseFilePath &&
            File.Exists(databaseFilePath))
        {
            InitializeAndRedirect(databaseFilePath);
        }

        return Task.CompletedTask;
    }

    private void InitializeAndRedirect(string databaseFilePath)
    {
        this.Data.Database.SetConnectionString($"Data Source={databaseFilePath}");
        this.Data.Initialize();

        this.NavigationManager.NavigateTo("/transactions");
    }

    private void Create_Click()
    {
        using (var dlg = new SaveFileDialog())
        {
            dlg.Filter = DatabaseFileDialogFilter;
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                var databaseFilePath = dlg.FileName;
                if (File.Exists(databaseFilePath))
                {
                    File.Delete(databaseFilePath);
                }

                SettingsStorage.WriteString(DatabaseFileKeyName, databaseFilePath);
                InitializeAndRedirect(databaseFilePath);
            }
        }
    }

    private void Load_Click()
    {
        using (var dlg = new OpenFileDialog())
        {
            dlg.Filter = DatabaseFileDialogFilter;
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                SettingsStorage.WriteString(DatabaseFileKeyName, dlg.FileName);
                InitializeAndRedirect(dlg.FileName);
            }
        }
    }
}
