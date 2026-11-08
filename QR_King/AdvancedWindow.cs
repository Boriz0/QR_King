using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;

namespace QR_King;

public sealed class AdvancedWindow : Window
{
    private readonly ComboBox correction = Choice(["L", "M", "Q", "H"]);
    private readonly ComboBox format = Choice(["PNG", "JPG", "SVG"]);
    private readonly ComboBox exportSize = Choice(["Auto (~512 px)", "256 × 256 px", "512 × 512 px", "1024 × 1024 px", "2048 × 2048 px", "Custom pixels per square"]);
    private readonly TextBox scale = new(), border = new(), version = new(), foreground = new(), background = new();
    private readonly TextBlock error = new() { Foreground = Brushes.Firebrick, TextWrapping = TextWrapping.Wrap };
    public QrOptions Options { get; private set; }
    public AdvancedWindow(QrOptions options)
    {
        AppTheme.Attach(this);
        error.SetResourceReference(TextBlock.ForegroundProperty, "ErrorBrush");
        Options = options.Copy();
        Title = "Advanced settings";
        Width = 440; Height = 630; MinWidth = 400; MinHeight = 400;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;

        FontFamily = new FontFamily("Segoe UI"); FontSize = 14;
        var root = new DockPanel { Margin = new Thickness(24) };
        root.SetResourceReference(Panel.BackgroundProperty, "SurfaceBrush");
        Content = root;
        var footer = new StackPanel();
        DockPanel.SetDock(footer, Dock.Bottom);
        root.Children.Add(footer);
        var panel = new StackPanel();
        root.Children.Add(new ScrollViewer { Content = panel, VerticalScrollBarVisibility = ScrollBarVisibility.Auto });
        panel.Children.Add(new TextBlock { Text = "Advanced settings", FontSize = 24, FontWeight = FontWeights.SemiBold });
        panel.Children.Add(new TextBlock { Text = "Apply regenerates the current text. Auto selects a size that fits the content.", TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 8, 0, 10) });
        AddRow(panel, "Error correction", correction, "L fits more data; M is a balanced default. Q and H tolerate more damage but can make the QR code larger and denser.");
        AddRow(panel, "Export size (independent of preview)", exportSize, "Presets produce an exact square pixel canvas. Whole QR squares stay sharp; spare space becomes extra background. Auto chooses a size near 512 px. SVG uses this as its nominal size and remains scalable.");
        exportSize.SelectionChanged += (_, _) => { scale.IsEnabled = exportSize.SelectedIndex == 5; if (scale.IsEnabled && scale.Text == "Auto") scale.Text = "12"; };
        AddRow(panel, "Custom pixels per QR square (2–16)", scale, "Only used with Custom. Larger values increase exported image size, not QR capacity. The final dimension depends on the QR version and border.");
        AddRow(panel, "Border in QR squares (4–16)", border, "The quiet zone separates the QR code from its surroundings. Keep at least four squares on every side so scanners can find it.");
        AddRow(panel, "QR version (Auto or 1–40)", version, "Version controls the number of squares in the code, not its display size. Auto chooses the smallest version that fits. A fixed version can be too small for your text.");
        AddRow(panel, "Foreground (#RRGGBB)", foreground, "Colour of the data squares. Use a dark colour with strong contrast against the background; black is the safest default.");
        AddRow(panel, "Background (#RRGGBB)", background, "Use a light background, such as white. The application theme never changes these QR colours.");
        AddRow(panel, "Default export format", format, "PNG is lossless and recommended. JPG uses quality 100 but is lossy. SVG stays sharp at any print size.");
        panel.Children.Add(new TextBlock { Text = "Higher correction can require a larger QR. Strong colour contrast and a border of at least four squares are required.", TextWrapping = TextWrapping.Wrap, FontSize = 12,  });
        footer.Children.Add(error);
        var openFolder = new Button
        {
            Content = "Open settings folder",
            ToolTip = Path.GetDirectoryName(UserPreferences.FilePath),
            HorizontalAlignment = HorizontalAlignment.Left,
            Padding = new Thickness(12, 8, 12, 8),
            Margin = new Thickness(0, 12, 0, 0)
        };
        openFolder.Click += (_, _) => OpenSettingsFolder();
        footer.Children.Add(openFolder);
        var buttons = new WrapPanel { Margin = new Thickness(0, 16, 0, 0) };
        var reset = new Button { Content = "Reset defaults", Padding = new Thickness(12, 8, 12, 8) };
        reset.Click += (_, _) => Populate(new QrOptions());
        var apply = new Button { Content = "Apply", IsDefault = true, Padding = new Thickness(16, 8, 16, 8), Margin = new Thickness(8, 0, 8, 0) };
        apply.Click += (_, _) => Apply();
        buttons.Children.Add(reset); buttons.Children.Add(apply); buttons.Children.Add(new Button { Content = "Cancel", IsCancel = true, Padding = new Thickness(12, 8, 12, 8) });
        footer.Children.Add(buttons);
        Populate(options);
    }
    private void Populate(QrOptions options)
    {
        correction.SelectedItem = options.ErrorCorrection; format.SelectedItem = options.Format;
        scale.Text = options.PixelsPerModule == 0 ? "Auto" : options.PixelsPerModule.ToString();
        exportSize.SelectedIndex = options.PixelsPerModule > 0 ? 5 : Array.IndexOf(new[] { 0, 256, 512, 1024, 2048 }, options.ExportSize);
        version.Text = options.Version == 0 ? "Auto" : options.Version.ToString();
        border.Text = options.Border.ToString(); foreground.Text = options.Foreground; background.Text = options.Background;
        error.Text = "";
    }
    private void OpenSettingsFolder()
    {
        string folder = Path.GetDirectoryName(UserPreferences.FilePath)!;
        try
        {
            Directory.CreateDirectory(folder);
            Process.Start(new ProcessStartInfo(folder) { UseShellExecute = true });
            error.Text = "";
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or Win32Exception or InvalidOperationException)
        {
            error.Text = $"Could not open the settings folder. Open this path in File Explorer: {folder}";
        }
    }
    private void Apply()
    {
        try
        {
            var options = new QrOptions { ErrorCorrection = (string)correction.SelectedItem, Format = (string)format.SelectedItem,
                ExportSize = exportSize.SelectedIndex is > 0 and < 5 ? new[] { 0, 256, 512, 1024, 2048 }[exportSize.SelectedIndex] : 0,
                PixelsPerModule = exportSize.SelectedIndex == 5 ? Parse(scale.Text) : 0, Version = Parse(version.Text), Border = Parse(border.Text),
                Foreground = foreground.Text.Trim(), Background = background.Text.Trim() };
            if (exportSize.SelectedIndex == 5 && options.PixelsPerModule == 0)
                throw new ArgumentException("Enter 2–16 pixels per square, or select Auto as the export size.");
            options.Validate(); Options = options; DialogResult = true;
        }
        catch (Exception ex) when (ex is ArgumentException or FormatException or OverflowException) { error.Text = ex.Message; }
    }
    private static int Parse(string text) => text.Trim().Equals("Auto", StringComparison.OrdinalIgnoreCase) ? 0 : int.Parse(text);
    private static ComboBox Choice(string[] items) => new() { ItemsSource = items };
    internal static void AddRow(Panel panel, string label, Control control, string? help = null)
    {
        panel.Children.Add(new TextBlock { Text = label, ToolTip = help, Margin = new Thickness(0, 9, 0, 3) });
        if (help != null)
        {
            control.ToolTip = new TextBlock { Text = help, TextWrapping = TextWrapping.Wrap, MaxWidth = 320 };
            ToolTipService.SetShowOnDisabled(control, true);
            System.Windows.Automation.AutomationProperties.SetHelpText(control, help);
        }
        control.Padding = new Thickness(6); panel.Children.Add(control);
        System.Windows.Automation.AutomationProperties.SetName(control, label);
    }
}
