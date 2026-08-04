using System.Globalization;

namespace CGTCalculator;

internal static class Exporter
{
    internal static async Task ExportAsync(string fileName, List<Transaction> transactions)
    {
        using var fs = File.Create(fileName);
        using var sr = new StreamWriter(fs);

        await WriteLineAsync(sr, nameof(Field.Date), nameof(Field.Type), nameof(Field.Symbol), nameof(Field.Quantity), nameof(Field.Value));
        foreach (var transaction in transactions)
        {
            await WriteLineAsync(
                sr,
                transaction.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                transaction.Type,
                transaction.Symbol,
                transaction.Quantity.ToString(CultureInfo.InvariantCulture),
                transaction.Value.ToString(CultureInfo.InvariantCulture));
        }
    }

    private static Task WriteLineAsync(StreamWriter sr, object v1, object v2, object v3, object v4, object v5)
    {
        return sr.WriteLineAsync(string.Join(",", Escape(v1), Escape(v2), Escape(v3), Escape(v4), Escape(v5)));
    }

    private static string Escape(object value)
    {
        var text = value.ToString() ?? string.Empty;
        return text.IndexOfAny([',', '"', '\r', '\n']) >= 0
            ? $"\"{text.Replace("\"", "\"\"")}\""
            : text;
    }
}
