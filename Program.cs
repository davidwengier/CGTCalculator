using Microsoft.AspNetCore.Components.WebView.WindowsForms;
using Microsoft.Extensions.DependencyInjection;

namespace CGTCalculator;

internal static class Program
{
    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
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
        return services.BuildServiceProvider();
    }

    private static Form CreateForm(ServiceProvider serviceProvider)
    {
        var form = new Form();

        var webView = new BlazorWebView();
        webView.Dock = DockStyle.Fill;
        form.Controls.Add(webView);

        webView.HostPage = "wwwroot\\index.html";
        webView.Services = serviceProvider;
        webView.RootComponents.Add<Counter>("#app");

        form.Size = new Size(800, 600);
        form.Text = "CGT Calculator";

        return form;
    }
}