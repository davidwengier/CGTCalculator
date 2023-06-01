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

        var form = CreateForm(services);

        Application.Run(form);
    }

    private static ServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection();
        services.AddWindowsFormsBlazorWebView();

        services.AddSingleton<DataSource>();

        services.AddQuickGridEntityFrameworkAdapter();
        services.AddDbContext<DataSource>(options => options.UseSqlite());

        return services.BuildServiceProvider();
    }

    private static Form CreateForm(ServiceProvider services)
        => new Form
        {
            Size = new Size(1500, 900),
            Text = "CGT Calculator",
            Icon = LoadIcon(),
            StartPosition = FormStartPosition.CenterScreen,
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

    private static Icon LoadIcon()
    {
        using (var stream = typeof(Program).Assembly.GetManifestResourceStream("CGTCalculator.icon.png"))
        {
            var bitmap = (Bitmap)Image.FromStream(stream!);
            return Icon.FromHandle(bitmap.GetHicon());
        }
    }
}
