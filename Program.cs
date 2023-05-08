using CGTCalculator.Pages;
using Microsoft.AspNetCore.Components.WebView.WindowsForms;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CGTCalculator;

internal static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();

        var services = ConfigureServices();

        var form = CreateForm(services);

        Application.Run(form);
    }

    private static ServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection();
        services.AddWindowsFormsBlazorWebView();

        services.AddSingleton<DataSource>();

        services.AddDbContext<DataSource>(options => options.UseSqlite("Data Source=CGTCalculator.db"));

        return services.BuildServiceProvider();
    }

    private static Form CreateForm(ServiceProvider services)
        => new Form
        {
            Size = new Size(1000, 800),
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