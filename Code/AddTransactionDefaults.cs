using System.Globalization;
using CGTCalculator.Code;

namespace CGTCalculator;

internal sealed record AddTransactionDefaults
{
    private const string SymbolKeyName = "LastTransactionSymbol";
    private const string DateKeyName = "LastTransactionDate";
    private const string TypeKeyName = "LastTransactionType";

    public string Symbol { get; init; } = string.Empty;

    public DateOnly Date { get; init; } = DateTime.Now.ToDateOnly();

    public TransactionType Type { get; init; } = TransactionType.Buy;

    public static AddTransactionDefaults Read()
    {
        var defaults = new AddTransactionDefaults
        {
            Symbol = SettingsStorage.ReadString(SymbolKeyName)
        };

        if (DateOnly.TryParseExact(
            SettingsStorage.ReadString(DateKeyName),
            "yyyy-MM-dd",
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out var date))
        {
            defaults = defaults with { Date = date };
        }

        if (Enum.TryParse<TransactionType>(SettingsStorage.ReadString(TypeKeyName), out var type))
        {
            defaults = defaults with { Type = type };
        }

        return defaults;
    }

    public static void Write(Transaction transaction)
    {
        SettingsStorage.WriteString(SymbolKeyName, transaction.Symbol);
        SettingsStorage.WriteString(DateKeyName, transaction.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
        SettingsStorage.WriteString(TypeKeyName, transaction.Type.ToString());
    }
}
