using Microsoft.AspNetCore.Components.WebView.WindowsForms;
using Microsoft.Extensions.DependencyInjection;

namespace CGTCalculator;

public partial class MainForm : Form
{
    public MainForm()
    {
        this.SuspendLayout();

        var webView = new BlazorWebView();
        webView.Dock = DockStyle.Fill;
        this.Controls.Add(webView);

        var services = new ServiceCollection();
        services.AddWindowsFormsBlazorWebView();
        webView.HostPage = "wwwroot\\index.html";
        webView.Services = services.BuildServiceProvider();
        webView.RootComponents.Add<Counter>("#app");

        this.Size = new Size(800, 600);
        this.Text = "CGT Calculator";

        this.ResumeLayout();
    }
}
