using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace QR_King;

public sealed class AdvancedWindow : Window
{
    private readonly ComboBox correction = Choice(["L", "M", "Q", "H"]);
    private readonly ComboBox format = Choice(["PNG", "JPG", "SVG"]);
    private readonly TextBox scale = new(), border = new(), version = new(), foreground = new(), background = new();
    private readonly CheckBox remember = new() { Content = "Remember text between sessions (stored locally)", Margin = new Thickness(0, 12, 0, 8) };
    private readonly TextBlock error = new() { Foreground = Brushes.Firebrick, TextWrapping = TextWrapping.Wrap };
    public QrOptions Options { get; private set; }
    public bool RememberText => remember.IsChecked == true;
    public AdvancedWindow(QrOptions options, bool rememberText)
    {
        Options = options.Copy();
        Title = "Advanced settings";
        Width = 440; Height = 630; MinWidth = 400; MinHeight = 400;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        Background = new SolidColorBrush(Color.FromRgb(246, 247, 249));
        FontFamily = new FontFamily("Segoe UI"); FontSize = 14;
        var root = new DockPanel { Background = Background, Margin = new Thickness(24) };
        Content = root;
        var footer = new StackPanel();
        DockPanel.SetDock(footer, Dock.Bottom);
        root.Children.Add(footer);
        var panel = new StackPanel();
        root.Children.Add(new ScrollViewer { Content = panel, VerticalScrollBarVisibility = ScrollBarVisibility.Auto });
        panel.Children.Add(new TextBlock { Text = "Advanced settings", FontSize = 24, FontWeight = FontWeights.SemiBold });
        panel.Children.Add(new TextBlock { Text = "Apply regenerates the current text. Auto selects a size that fits the content.", TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 8, 0, 10) });
        AddRow(panel, "Error correction", correction);
        AddRow(panel, "Pixels per QR square (Auto or 2–16)", scale);
        AddRow(panel, "Border in QR squares (4–16)", border);
        AddRow(panel, "QR version (Auto or 1–40)", version);
        AddRow(panel, "Foreground (#RRGGBB)", foreground);
        AddRow(panel, "Background (#RRGGBB)", background);
        AddRow(panel, "Default export format", format);
        panel.Children.Add(remember);
        panel.Children.Add(new TextBlock { Text = "Higher correction can require a larger QR. Strong colour contrast and a border of at least four squares are required.", TextWrapping = TextWrapping.Wrap, FontSize = 12, Foreground = Brushes.DimGray });
        footer.Children.Add(error);
        var buttons = new WrapPanel { Margin = new Thickness(0, 16, 0, 0) };
        var reset = new Button { Content = "Reset defaults", Padding = new Thickness(12, 8, 12, 8) };
        reset.Click += (_, _) => Populate(new QrOptions(), false);
        var apply = new Button { Content = "Apply", IsDefault = true, Padding = new Thickness(16, 8, 16, 8), Margin = new Thickness(8, 0, 8, 0) };
        apply.Click += (_, _) => Apply();
        buttons.Children.Add(reset); buttons.Children.Add(apply); buttons.Children.Add(new Button { Content = "Cancel", IsCancel = true, Padding = new Thickness(12, 8, 12, 8) });
        footer.Children.Add(buttons);
        Populate(options, rememberText);
    }
    private void Populate(QrOptions options, bool rememberText)
    {
        correction.SelectedItem = options.ErrorCorrection; format.SelectedItem = options.Format;
        scale.Text = options.PixelsPerModule == 0 ? "Auto" : options.PixelsPerModule.ToString();
        version.Text = options.Version == 0 ? "Auto" : options.Version.ToString();
        border.Text = options.Border.ToString(); foreground.Text = options.Foreground; background.Text = options.Background;
        remember.IsChecked = rememberText; error.Text = "";
    }
    private void Apply()
    {
        try
        {
            var options = new QrOptions { ErrorCorrection = (string)correction.SelectedItem, Format = (string)format.SelectedItem,
                PixelsPerModule = Parse(scale.Text), Version = Parse(version.Text), Border = Parse(border.Text),
                Foreground = foreground.Text.Trim(), Background = background.Text.Trim() };
            options.Validate(); Options = options; DialogResult = true;
        }
        catch (Exception ex) when (ex is ArgumentException or FormatException or OverflowException) { error.Text = ex.Message; }
    }
    private static int Parse(string text) => text.Trim().Equals("Auto", StringComparison.OrdinalIgnoreCase) ? 0 : int.Parse(text);
    private static ComboBox Choice(string[] items) => new() { ItemsSource = items };
    internal static void AddRow(Panel panel, string label, Control control)
    {
        panel.Children.Add(new TextBlock { Text = label, Margin = new Thickness(0, 9, 0, 3) });
        control.Padding = new Thickness(6); panel.Children.Add(control);
        System.Windows.Automation.AutomationProperties.SetName(control, label);
    }
}
