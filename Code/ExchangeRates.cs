using System.Text.Json;

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
                    s_currencyCacheCreated = now;
                    s_instance = new ExchangeRates();

                    try
                    {
                        string json = s_httpClient.GetStringAsync(Endpoint).Result;
                        s_instance.Rates = JsonSerializer.Deserialize<Dictionary<string, ExchangeRate>>(json)!;
                    }
                    catch
                    {
                    }
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

        rate = this.Rates.Values.First(r =>
            r.code.IndexOf(currency, StringComparison.OrdinalIgnoreCase) != -1 ||
            r.name.IndexOf(currency, StringComparison.OrdinalIgnoreCase) != -1);

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
    public required float rate { get; set; }
    public required float inverseRate { get; set; }
}
#pragma warning restore IDE1006 // Naming Styles
