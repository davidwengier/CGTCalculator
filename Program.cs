using CGTCalculator.Pages;
using Microsoft.AspNetCore.Components.WebView.WindowsForms;
using Microsoft.Extensions.DependencyInjection;

namespace CGTCalculator;

internal static class Program
{
    [STAThread]
    public static void Main()
    {
        ApplicationConfiguration.Initialize();

        var services = ConfigureServices();

        ConfigureDatabase(services);

        var form = CreateForm(services);

        Application.Run(form);
    }

    private static ServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection();
        services.AddWindowsFormsBlazorWebView();

        services.AddSingleton<DataSource>();

        services.AddQuickGridEntityFrameworkAdapter();
        services.AddDbContext<DataSource>(options => options.UseSqlite("Data Source=CGTCalculator.db"));

        return services.BuildServiceProvider();
    }

    private static void ConfigureDatabase(ServiceProvider services)
    {
        var database = services.GetRequiredService<DataSource>();
        database.Database.EnsureCreated();
        _ = database.GetDatabaseInfo();
    }

    private static Form CreateForm(ServiceProvider services)
        => new Form
        {
            Size = new Size(1500, 900),
            Text = "CGT Calculator",
            Controls =
            {
                new BlazorWebView
                {
                    Dock = DockStyle.Fill,
                    HostPage = @"wwwroot\index.html",
                    Services = services,
                    RootComponents =
                    {
                        new RootComponent("#app", typeof(Main), null)
                    }
                }
            }
        };
}
