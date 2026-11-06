using System.Reflection;
using System.Diagnostics;
using System.ComponentModel;
using System.Windows;
using System.Windows.Navigation;
namespace QR_King;
public partial class AboutWindow : Window
{
    private void GitHub_RequestNavigate(object sender, RequestNavigateEventArgs e)
    {
        e.Handled = true;
        try
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
        }
        catch (Exception ex) when (ex is Win32Exception or InvalidOperationException)
        {
            MessageBox.Show(this, "Could not open your browser. Open https://github.com/Boriz0/QR_King manually.", "QR King", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    public AboutWindow()
    {
        InitializeComponent();
        string version = typeof(AboutWindow).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "0.1.0";
        string[] parts = version.Split('+', 2);
        VersionText.Text = $"Version: {parts[0]}";
        RevisionText.Text = $"Build revision: {(parts.Length > 1 ? parts[1] : "local build (no Git revision)")}";
    }
}
