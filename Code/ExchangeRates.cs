using System.Text.Json;
using System.Text.Json.Serialization;

namespace CGTCalculator;

// Copy of https://github.com/KirillOsenkov/QuickInfo/blob/main/src/QuickInfo/Processors/Converters/Currency.cs
public class ExchangeRates
{
    private const string Endpoint = @"https://www.floatrates.com/daily/usd.json";
    private static readonly HttpClient s_httpClient = new HttpClient();

    private static DateTime s_currencyCacheCreated = DateTime.UtcNow;

    private static ExchangeRates? s_instance;
    public static ExchangeRates Instance
    {
        get
        {
            var now = DateTime.UtcNow;
            if (s_instance == null ||
                s_currencyCacheCreated == default ||
                now - s_currencyCacheCreated > TimeSpan.FromDays(1))
            {
                lock (s_httpClient)
                {
                    string json = s_httpClient.GetStringAsync(Endpoint).GetAwaiter().GetResult();
                    var rates = JsonSerializer.Deserialize<Dictionary<string, ExchangeRate>>(json)
                        ?? throw new InvalidDataException("The exchange-rate service returned no data.");

                    s_instance = new ExchangeRates { Rates = rates };
                    s_currencyCacheCreated = now;
                }
            }

            return s_instance;
        }
    }

    public ExchangeRate Get(string currency)
    {
        if (this.Rates.TryGetValue(currency, out var rate))
        {
            return rate;
        }

        rate = this.Rates.Values.FirstOrDefault(r =>
            r.code.IndexOf(currency, StringComparison.OrdinalIgnoreCase) != -1 ||
            r.name.IndexOf(currency, StringComparison.OrdinalIgnoreCase) != -1)
            ?? throw new KeyNotFoundException($"No exchange rate was found for '{currency}'.");

        return rate;
    }

    public Dictionary<string, ExchangeRate> Rates { get; private set; } = new Dictionary<string, ExchangeRate>();
}

#pragma warning disable IDE1006 // Naming Styles
public class ExchangeRate
{
    public required string code { get; set; }
    public required string alphaCode { get; set; }
    public required string numericCode { get; set; }
    public required string name { get; set; }
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public required decimal rate { get; set; }

    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public required decimal inverseRate { get; set; }
}
#pragma warning restore IDE1006 // Naming Styles
