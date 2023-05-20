using Microsoft.AspNetCore.Components.Forms;

namespace CGTCalculator.Pages;

public partial class Import
{
    private bool _buttonClicked;
    private IBrowserFile? _lastSelectedFile;

    private readonly List<Field> _columns = new(new[] { Field.Date, Field.Quantity, Field.Value });
    private List<string>? _fileColumns;

    public void Back_Click()
    {
        this.NavigationManager.NavigateTo("/");
    }

    public async Task File_Change(InputFileChangeEventArgs e)
    {
        using var inputStream = e.File.OpenReadStream();
        using var streamReader = new StreamReader(inputStream);
        var line = await streamReader.ReadLineAsync().ConfigureAwait(false);
        if (line is null)
        {
            return;
        }

        _lastSelectedFile = e.File;
        _columns.Clear();
        _fileColumns = new List<string>();
        foreach (var col in line.Split(','))
        {
            _fileColumns.Add(col);
            _columns.Add(TryMatchColumnName(col));
        }
    }

    public async Task DoImport()
    {
        _buttonClicked = true;
        if (_lastSelectedFile is not null)
        {
            using var inputStream = _lastSelectedFile.OpenReadStream();
            using var streamReader = new StreamReader(inputStream);
            _ = await streamReader.ReadLineAsync().ConfigureAwait(false);
            string? line;
            while ((line = await streamReader.ReadLineAsync().ConfigureAwait(false)) != null)
            {
                var transaction = new Transaction();
                transaction.Id = Guid.NewGuid();
                var data = line.Split(',');
                for (int i = 0; i < _columns.Count; i++)
                {
                    SetField(transaction, _columns[i], data[i]);
                }

                if (transaction.Type == TransactionType.Sell)
                {
                    transaction.Quantity *= -1;
                    transaction.Value *= -1;
                }

                await this.DataSource.Transactions.AddAsync(transaction).ConfigureAwait(false);
            }

            await this.DataSource.SaveChangesAsync().ConfigureAwait(false);
            this.NavigationManager.NavigateTo("/");
        }
    }

    private void SetField(Transaction transaction, Field field, string value)
    {
        switch (field)
        {
            case Field.Symbol:
                transaction.Symbol = value;
                break;
            case Field.Date:
                transaction.Date = DateOnly.Parse(value);
                break;
            case Field.Quantity:
                transaction.Quantity = Math.Abs(decimal.Parse(value));
                break;
            case Field.Value:
                transaction.Value = Math.Abs(decimal.Parse(value));
                break;
            case Field.Type:
                transaction.Type = Enum.Parse<TransactionType>(value);
                break;
        }
    }

    private static Field TryMatchColumnName(string columnFromFile) => columnFromFile switch
    {
        "Symbol" or "Stock" or "Ticker" => Field.Symbol,
        "Date" or "DateTime" or "TransactionDate" => Field.Date,
        "Qty" or "Quantity" or "Units" => Field.Quantity,
        "$" or "Price" or "UnitPrice" or "Amount" or "Value" => Field.Value,
        "Type" or "TransactionType" => Field.Type,
        _ => Field.Ignore
    };
}
