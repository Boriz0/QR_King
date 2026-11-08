using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace QR_King;

public static class QrTemplates
{
    private static string WifiEscape(string value) => value.Replace("\\", "\\\\").Replace(";", "\\;").Replace(",", "\\,").Replace(":", "\\:").Replace("\"", "\\\"");
    private static string CardEscape(string value) => value.Replace("\\", "\\\\").Replace("\r\n", "\n").Replace("\r", "\n").Replace("\n", "\\n").Replace(";", "\\;").Replace(",", "\\,");
    public static string Wifi(string ssid, string password, string security, bool hidden)
    {
        if (string.IsNullOrWhiteSpace(ssid)) throw new ArgumentException("Enter the Wi-Fi network name.");
        if (!new[] { "WPA", "WEP", "nopass" }.Contains(security)) throw new ArgumentException("Choose a Wi-Fi security type.");
        if (security != "nopass" && password.Length == 0) throw new ArgumentException("Enter the Wi-Fi password, or select Open network.");
        return $"WIFI:T:{security};S:{WifiEscape(ssid)};P:{WifiEscape(security == "nopass" ? "" : password)};H:{hidden.ToString().ToLowerInvariant()};;";
    }
    public static string Contact(string first, string last, string phone, string email, string organization)
    {
        if (string.IsNullOrWhiteSpace(first + last)) throw new ArgumentException("Enter a first or last name.");
        return $"BEGIN:VCARD\r\nVERSION:3.0\r\nN:{CardEscape(last)};{CardEscape(first)};;;\r\nFN:{CardEscape((first + " " + last).Trim())}\r\nORG:{CardEscape(organization)}\r\nTEL:{CardEscape(phone)}\r\nEMAIL:{CardEscape(email)}\r\nEND:VCARD";
    }
}

public sealed class TemplateWindow : Window
{
    public string Payload { get; private set; } = "";
    public TemplateWindow()
    {
        AppTheme.Attach(this);
        Title = "QR templates"; Width = 420; Height = 560; MinWidth = 360; MinHeight = 420;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        FontFamily = new FontFamily("Segoe UI"); FontSize = 14;
        var root = new DockPanel { Margin = new Thickness(20) }; Content = root; root.SetResourceReference(Panel.BackgroundProperty, "SurfaceBrush");
        var footer = new StackPanel(); DockPanel.SetDock(footer, Dock.Bottom); root.Children.Add(footer);
        var error = new TextBlock { Foreground = Brushes.Firebrick, TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 8, 0, 8) };
        error.SetResourceReference(TextBlock.ForegroundProperty, "ErrorBrush");
        footer.Children.Add(error);
        footer.Children.Add(new TextBlock { Text = "Using a template switches to manual mode. The generated content will be visible in the main window.", TextWrapping = TextWrapping.Wrap, FontSize = 12, Margin = new Thickness(0, 0, 0, 10) });
        var buttons = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right };
        var use = new Button { Content = "Use template", Padding = new Thickness(14, 8, 14, 8), IsDefault = true };
        buttons.Children.Add(use); buttons.Children.Add(new Button { Content = "Cancel", IsCancel = true, Padding = new Thickness(14, 8, 14, 8), Margin = new Thickness(8, 0, 0, 0) }); footer.Children.Add(buttons);
        var tabs = new TabControl(); root.Children.Add(tabs);
        var wifi = new StackPanel { Margin = new Thickness(10) }; var contact = new StackPanel { Margin = new Thickness(10) };
        tabs.Items.Add(new TabItem { Header = "Wi-Fi", Content = new ScrollViewer { Content = wifi, VerticalScrollBarVisibility = ScrollBarVisibility.Auto } });
        tabs.Items.Add(new TabItem { Header = "Contact", Content = new ScrollViewer { Content = contact, VerticalScrollBarVisibility = ScrollBarVisibility.Auto } });
        var ssid = new TextBox(); var password = new PasswordBox();
        var security = new ComboBox { ItemsSource = new[] { "WPA / WPA2", "WEP", "Open network" }, SelectedIndex = 0 };
        var hidden = new CheckBox { Content = "Hidden network", Margin = new Thickness(0, 12, 0, 0) };
        AdvancedWindow.AddRow(wifi, "Network name (SSID)", ssid); AdvancedWindow.AddRow(wifi, "Security", security); AdvancedWindow.AddRow(wifi, "Password", password); wifi.Children.Add(hidden);
        security.SelectionChanged += (_, _) => password.IsEnabled = security.SelectedIndex != 2;
        var first = new TextBox(); var last = new TextBox(); var phone = new TextBox(); var email = new TextBox(); var organization = new TextBox();
        AdvancedWindow.AddRow(contact, "First name", first); AdvancedWindow.AddRow(contact, "Last name", last); AdvancedWindow.AddRow(contact, "Phone", phone); AdvancedWindow.AddRow(contact, "Email", email); AdvancedWindow.AddRow(contact, "Organization", organization);
        use.Click += (_, _) =>
        {
            try
            {
                Payload = tabs.SelectedIndex == 0 ? QrTemplates.Wifi(ssid.Text, password.Password, new[] { "WPA", "WEP", "nopass" }[security.SelectedIndex], hidden.IsChecked == true)
                    : QrTemplates.Contact(first.Text, last.Text, phone.Text, email.Text, organization.Text);
                DialogResult = true;
            }
            catch (ArgumentException ex) { error.Text = ex.Message; }
        };
    }
}
