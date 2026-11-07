using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml.Linq;
using QR_King;

internal static class FeatureTests
{
    public static void Run()
    {
        const string payload = "https://example.com/Žilina?q=hello 😀";
        foreach (string correction in new[] { "L", "M", "Q", "H" })
        {
            var qr = QrImages.Create(payload, new QrOptions { ErrorCorrection = correction, PixelsPerModule = 4, Border = 6, Foreground = "#102030", Background = "#FFF8E0" });
            foreach (string format in new[] { "PNG", "JPG" })
            {
                using var stream = new MemoryStream(QrImages.Export(qr, format));
                QrImages.Verify(BitmapDecoder.Create(stream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad).Frames[0], payload);
            }
            var svg = XDocument.Parse(System.Text.Encoding.UTF8.GetString(QrImages.Export(qr, "SVG")));
            if (svg.Root?.Attribute("viewBox")?.Value != $"0 0 {qr.Squares} {qr.Squares}") throw new Exception("SVG dimensions incorrect");
            // Reconstruct SVG square paths independently and decode the exported geometry.
            int size = qr.Squares * 4;
            byte[] pixels = Enumerable.Repeat((byte)255, size * size * 3).ToArray();
            string path = svg.Root!.Element(XName.Get("path", "http://www.w3.org/2000/svg"))!.Attribute("d")!.Value;
            foreach (System.Text.RegularExpressions.Match match in System.Text.RegularExpressions.Regex.Matches(path, @"M(\d+) (\d+)h1v1h-1z"))
            {
                int x = int.Parse(match.Groups[1].Value), y = int.Parse(match.Groups[2].Value);
                if (x < 6 || y < 6 || x >= qr.Squares - 6 || y >= qr.Squares - 6) throw new Exception("SVG quiet zone violated");
                for (int dy = 0; dy < 4; dy++) for (int dx = 0; dx < 4; dx++)
                { int i = ((y * 4 + dy) * size + x * 4 + dx) * 3; pixels[i] = pixels[i + 1] = pixels[i + 2] = 0; }
            }
            QrImages.Verify(BitmapSource.Create(size, size, 96, 96, PixelFormats.Rgb24, null, pixels, size * 3), payload);
        }
        var fixedQr = QrImages.Create("small", new QrOptions { Version = 5, Border = 4 });
        if (fixedQr.Squares != 45) throw new Exception("Fixed QR version not respected");
        foreach (var invalid in new[] { new QrOptions { Border = 3 }, new QrOptions { PixelsPerModule = 100 }, new QrOptions { Version = 41 }, new QrOptions { Foreground = "#FFFFFF" }, new QrOptions { Background = "bad" } })
        {
            try { invalid.Validate(); throw new Exception("Invalid settings accepted"); } catch (ArgumentException) { }
        }
        string wifi = QrTemplates.Wifi("Office;Guest", "a:b\\c", "WPA", true);
        if (!wifi.Contains("S:Office\\;Guest;") || !wifi.Contains("P:a\\:b\\\\c;") || !wifi.Contains("H:true;")) throw new Exception("Wi-Fi escaping failed");
        string contact = QrTemplates.Contact("Boris", "Vacula", "+421123", "b@example.com", "A;B\nC");
        if (!contact.Contains("N:Vacula;Boris;;;") || !contact.Contains("ORG:A\\;B\\nC")) throw new Exception("Contact escaping failed");
        QrImages.Create(wifi, new()); QrImages.Create(contact, new());
        string file = Path.GetFullPath("artifacts/test-settings.json");
        var prefs = new UserPreferences { SavedText = "private", RememberText = false, FitWindow = true, LiveUpdates = false, Qr = new() { Format = "SVG" } };
        prefs.Save(file);
        if (File.ReadAllText(file).Contains("private") || !UserPreferences.Load(file).FitWindow || UserPreferences.Load(file).Qr.Format != "SVG") throw new Exception("Preferences persistence/privacy failed");
        prefs.RememberText = true; prefs.SavedText = "opted in"; prefs.Save(file);
        if (UserPreferences.Load(file).SavedText != "opted in") throw new Exception("Opt-in text persistence failed");
        prefs.RememberText = false; prefs.Save(file);
        if (UserPreferences.Load(file).SavedText != null) throw new Exception("Opt-out failed to erase text");
        prefs.RememberText = true; prefs.SavedText = "delete-me"; prefs.Save(file);
        prefs.DeleteSavedText(file);
        if (prefs.RememberText || UserPreferences.Load(file).RememberText || File.ReadAllText(file).Contains("delete-me"))
            throw new Exception("Delete saved text failed to remove persisted content and disable remembering");
        prefs.RememberText = true; prefs.SavedText = "dialog-delete"; prefs.Save(file);
        var privacyDialog = new AdvancedWindow(new(), true, () => prefs.DeleteSavedText(file));
        Render(privacyDialog, "tests/privacy-controls-preview.png", 440, 630);
        ((DockPanel)privacyDialog.Content).Children.OfType<ScrollViewer>().Single().ScrollToEnd();
        Render(privacyDialog, "tests/privacy-controls-preview.png", 440, 630);
        var deleteButton = FindButton((FrameworkElement)privacyDialog.Content, "DeleteSavedTextButton") ?? throw new Exception("Delete button missing");
        deleteButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        if (privacyDialog.RememberText || UserPreferences.Load(file).RememberText || File.ReadAllText(file).Contains("dialog-delete"))
            throw new Exception("Immediate deletion depended on applying the dialog");
        // Cancel/close without Apply must not restore saved text.
        privacyDialog.Close();
        if (UserPreferences.Load(file).SavedText != null) throw new Exception("Cancelling restored deleted text");
        File.WriteAllText(file, "broken json");
        if (UserPreferences.Load(file).Qr.Format != "PNG") throw new Exception("Corrupt settings fallback failed");
        File.Delete(file);
        var window = new MainWindow(false);
        typeof(MainWindow).GetMethod("ApplyClipboardText", BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(window, [payload]);
        ((CheckBox)window.FindName("FitWindow")).IsChecked = true;
        if (window.Width > SystemParameters.WorkArea.Width || window.Height > SystemParameters.WorkArea.Height) throw new Exception("Auto-fit exceeded screen");
        ((CheckBox)window.FindName("PinWindow")).IsChecked = true;
        if (!window.Topmost) throw new Exception("Always on top failed");
        typeof(MainWindow).GetMethod("Clear_Click", BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(window, [window, new RoutedEventArgs()]);
        if (((TextBox)window.FindName("ContentText")).Text.Length != 0 || ((Image)window.FindName("QrImage")).Source != null || ((Button)window.FindName("SaveButton")).IsEnabled) throw new Exception("Clear did not reset content");
        Render(new AdvancedWindow(new(), false), "tests/advanced-preview.png", 440, 630);
        Render(new TemplateWindow(), "tests/templates-preview.png", 420, 560);
        Console.WriteLine("PASS: all correction levels; PNG/JPG/SVG decoded; fixed version, colours, border and invalid settings; templates; preferences/privacy; fit, pin and clear.");
    }
    static void Render(Window window, string path, int width, int height)
    {
        var element = (FrameworkElement)window.Content;
        element.Measure(new Size(width, height)); element.Arrange(new Rect(0, 0, width, height)); element.UpdateLayout();
        var bitmap = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32); bitmap.Render(element);
        var encoder = new PngBitmapEncoder(); encoder.Frames.Add(BitmapFrame.Create(bitmap));
        using var output = File.Create(path); encoder.Save(output);
    }
    static Button? FindButton(DependencyObject parent, string name)
    {
        if (parent is Button button && button.Name == name) return button;
        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
        {
            var result = FindButton(VisualTreeHelper.GetChild(parent, i), name);
            if (result != null) return result;
        }
        return null;
    }
}
