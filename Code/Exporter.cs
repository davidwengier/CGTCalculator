namespace CGTCalculator;

internal static class Exporter
{
    internal static async Task ExportAsync(string fileName, List<Transaction> transactions)
    {
        using var fs = File.OpenWrite(fileName);
        using var sr = new StreamWriter(fs);

        await WriteLineAsync(sr, nameof(Field.Date), nameof(Field.Type), nameof(Field.Symbol), nameof(Field.Quantity), nameof(Field.Value));
        foreach (var transaction in transactions)
        {
            await WriteLineAsync(sr, transaction.Date, transaction.Type, transaction.Symbol, transaction.Quantity, transaction.Value);
        }
    }

    private static Task WriteLineAsync(StreamWriter sr, object v1, object v2, object v3, object v4, object v5)
    {
        return sr.WriteLineAsync(string.Join(",", v1, v2, v3, v4, v5));
    }
}
