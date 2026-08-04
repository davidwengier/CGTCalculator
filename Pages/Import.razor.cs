using System.Globalization;
using System.Text;
using Microsoft.AspNetCore.Components.Forms;

namespace CGTCalculator.Pages;

public partial class Import
{
    private const long MaxImportFileSize = 50 * 1024 * 1024;

    private bool _buttonClicked;
    private bool _isImporting;
    private string? _importError;
    private IBrowserFile? _lastSelectedFile;

    private readonly List<Field> _columns = new(new[] { Field.Date, Field.Quantity, Field.Value });
    private List<string>? _fileColumns;

    public void Back_Click()
    {
        this.NavigationManager.NavigateTo("/");
    }

    public async Task File_Change(InputFileChangeEventArgs e)
    {
        _buttonClicked = false;
        _importError = null;

        try
        {
            using var inputStream = e.File.OpenReadStream(MaxImportFileSize);
            using var streamReader = new StreamReader(inputStream);
            var line = await streamReader.ReadLineAsync();
            if (line is null)
            {
                _lastSelectedFile = null;
                _importError = "The selected file is empty.";
                return;
            }

            _lastSelectedFile = e.File;
            _columns.Clear();
            _fileColumns = ParseCsvLine(line);
            foreach (var col in _fileColumns)
            {
                _columns.Add(TryMatchColumnName(col));
            }
        }
        catch (Exception ex) when (ex is IOException or InvalidDataException)
        {
            _lastSelectedFile = null;
            _importError = $"Unable to read the selected file: {ex.Message}";
        }
    }

    public async Task DoImport()
    {
        _buttonClicked = true;
        _importError = null;
        if (_lastSelectedFile is null)
        {
            return;
        }

        _isImporting = true;
        try
        {
            using var inputStream = _lastSelectedFile.OpenReadStream(MaxImportFileSize);
            using var streamReader = new StreamReader(inputStream);
            _ = await streamReader.ReadLineAsync();

            var transactions = new List<Transaction>();
            string? line;
            var lineNumber = 1;
            while ((line = await streamReader.ReadLineAsync()) != null)
            {
                lineNumber++;
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                var data = ParseCsvLine(line);
                if (data.Count != _columns.Count)
                {
                    throw new InvalidDataException(
                        $"Line {lineNumber} has {data.Count} column(s), but the header has {_columns.Count}.");
                }

                var transaction = new Transaction { Id = Guid.NewGuid() };
                for (int i = 0; i < _columns.Count; i++)
                {
                    SetField(transaction, _columns[i], data[i], lineNumber);
                }

                if (transaction.Type == TransactionType.Sell)
                {
                    transaction.Quantity *= -1;
                    transaction.Value *= -1;
                }

                transactions.Add(transaction);
            }

            await this.DataSource.Transactions.AddRangeAsync(transactions);
            await this.DataSource.SaveChangesAsync();
            this.NavigationManager.NavigateTo("/");
        }
        catch (Exception ex) when (ex is IOException or InvalidDataException or FormatException or DbUpdateException)
        {
            this.DataSource.ChangeTracker.Clear();
            _importError = $"Import failed: {ex.Message}";
        }
        finally
        {
            _isImporting = false;
        }
    }

    private static void SetField(Transaction transaction, Field field, string value, int lineNumber)
    {
        switch (field)
        {
            case Field.Symbol:
                transaction.Symbol = value.Trim();
                break;
            case Field.Date:
                if (!DateOnly.TryParse(value, CultureInfo.CurrentCulture, DateTimeStyles.None, out var date) &&
                    !DateOnly.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
                {
                    throw new FormatException($"Line {lineNumber} contains an invalid date: '{value}'.");
                }

                transaction.Date = date;
                break;
            case Field.Quantity:
                transaction.Quantity = Math.Abs(ParseDecimal(value, lineNumber, "quantity"));
                break;
            case Field.Value:
                transaction.Value = Math.Abs(ParseDecimal(value, lineNumber, "value"));
                break;
            case Field.Type:
                if (!Enum.TryParse<TransactionType>(value, ignoreCase: true, out var type))
                {
                    throw new FormatException($"Line {lineNumber} contains an invalid transaction type: '{value}'.");
                }

                transaction.Type = type;
                break;
        }
    }

    private static decimal ParseDecimal(string value, int lineNumber, string fieldName)
    {
        if (decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out var result) ||
            decimal.TryParse(value, NumberStyles.Number, CultureInfo.CurrentCulture, out result))
        {
            return result;
        }

        throw new FormatException($"Line {lineNumber} contains an invalid {fieldName}: '{value}'.");
    }

    private static List<string> ParseCsvLine(string line)
    {
        var values = new List<string>();
        var value = new StringBuilder();
        var inQuotes = false;

        for (var i = 0; i < line.Length; i++)
        {
            var character = line[i];
            if (character == '"')
            {
                if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                {
                    value.Append('"');
                    i++;
                }
                else
                {
                    inQuotes = !inQuotes;
                }
            }
            else if (character == ',' && !inQuotes)
            {
                values.Add(value.ToString());
                value.Clear();
            }
            else
            {
                value.Append(character);
            }
        }

        if (inQuotes)
        {
            throw new InvalidDataException("The CSV contains an unterminated quoted value.");
        }

        values.Add(value.ToString());
        return values;
    }

    private static Field TryMatchColumnName(string columnFromFile) => columnFromFile.Trim() switch
    {
        "Symbol" or "Stock" or "Ticker" => Field.Symbol,
        "Date" or "DateTime" or "TransactionDate" => Field.Date,
        "Qty" or "Quantity" or "Units" => Field.Quantity,
        "$" or "Price" or "UnitPrice" or "Amount" or "Value" => Field.Value,
        "Type" or "TransactionType" => Field.Type,
        _ => Field.Ignore
    };
}
