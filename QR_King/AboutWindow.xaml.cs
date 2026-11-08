using System.Reflection;
using System.Diagnostics;
using System.ComponentModel;
using System.Windows;
using System.Windows.Navigation;
namespace QR_King;
public partial class AboutWindow : Window
{
    private void Privacy_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            string path = System.IO.Path.Combine(AppContext.BaseDirectory, "PRIVACY.md");
            var text = new System.Windows.Controls.TextBox { Text = System.IO.File.ReadAllText(path), IsReadOnly = true, TextWrapping = TextWrapping.Wrap, VerticalScrollBarVisibility = System.Windows.Controls.ScrollBarVisibility.Auto, Padding = new Thickness(16), BorderThickness = new Thickness(0) };
            var privacy = new Window { Title = "Privacy — QR King", Owner = this, Width = 560, Height = 580, MinWidth = 360, MinHeight = 300, WindowStartupLocation = WindowStartupLocation.CenterOwner, Content = text };
            AppTheme.Attach(privacy);
            privacy.ShowDialog();
        }
        catch (Exception ex) when (ex is System.IO.IOException or UnauthorizedAccessException)
        {
            MessageBox.Show(this, "The local privacy document could not be opened. It is also available as PRIVACY.md in the GitHub repository.", "QR King", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

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
        AppTheme.Attach(this);
        InitializeComponent();
        string version = typeof(AboutWindow).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "0.1.0";
        string[] parts = version.Split('+', 2);
        VersionText.Text = $"Version: {parts[0]}";
        RevisionText.Text = $"Build revision: {(parts.Length > 1 ? parts[1] : "local build (no Git revision)")}";
    }
}
