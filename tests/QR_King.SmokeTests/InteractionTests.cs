using QR_King;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

static class InteractionTests
{
    const BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;
    static void Invoke(MainWindow w, string method, params object?[] args) => typeof(MainWindow).GetMethod(method, Private)!.Invoke(w, args);
    static T Control<T>(MainWindow w, string name) => (T)w.FindName(name);
    public static void Run()
    {
        foreach (int size in new[] { 256, 512, 1024, 2048 })
        {
            var qr = QrImages.Create("Exact export size", new QrOptions { ExportSize = size });
            if (qr.Image.PixelWidth != size || qr.Image.PixelHeight != size) throw new Exception("Preset dimensions incorrect");
            QrImages.Verify(qr.Image, qr.Text);
            foreach (string format in new[] { "PNG", "JPG" })
            {
                using var stream = new MemoryStream(QrImages.Export(qr, format));
                var decoded = BitmapDecoder.Create(stream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad).Frames[0];
                if (decoded.PixelWidth != size) throw new Exception("Export size changed");
                QrImages.Verify(decoded, qr.Text);
            }
        }
        try { QrImages.Create("dense", new QrOptions { Version = 40, ExportSize = 256 }); throw new Exception("Dense preset accepted"); }
        catch (ArgumentException ex) { if (!ex.Message.Contains("larger")) throw; }

        string path = Path.GetFullPath("artifacts/import-test.txt");
        const string payload = "Imported Žilina 😀\nSecond line";
        foreach (Encoding encoding in new Encoding[] { new UTF8Encoding(false), Encoding.UTF8, Encoding.Unicode, Encoding.BigEndianUnicode, Encoding.UTF32 })
        {
            File.WriteAllText(path, payload, encoding);
            if (TextFileImport.Read(path) != payload) throw new Exception("Unicode file import failed");
        }
        var window = new MainWindow(false);
        Invoke(window, "ImportTextFile", path);
        var live = Control<CheckBox>(window, "LiveUpdates");
        var content = Control<TextBox>(window, "ContentText");
        var preview = Control<Image>(window, "QrImage");
        var save = Control<Button>(window, "SaveButton");
        var stale = Control<Border>(window, "StaleOverlay");
        if (live.IsChecked != false || content.Text != payload || !save.IsEnabled) throw new Exception("File import did not enter manual mode");
        foreach (byte[] bad in new[] { new byte[TextFileImport.MaximumBytes + 1], new byte[] { 0, 1, 2 }, new byte[] { 0xff, 0xff } })
        {
            File.WriteAllBytes(path, bad);
            Invoke(window, "ImportTextFile", path);
            if (content.Text != payload || !save.IsEnabled || Control<TextBlock>(window, "ErrorText").Visibility != Visibility.Visible)
                throw new Exception("Bad import damaged existing content or lacked error");
        }
        var original = preview.Source;
        content.Text = "edited";
        if (stale.Visibility != Visibility.Visible || save.IsEnabled) throw new Exception("Missing stale overlay");
        content.Text = payload;
        if (stale.Visibility != Visibility.Collapsed || !save.IsEnabled || preview.Source != original) throw new Exception("Reverted draft is not restored");
        content.Text = "edited again";
        Control<Button>(window, "RegenerateButton").RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        if (stale.Visibility != Visibility.Collapsed || !save.IsEnabled) throw new Exception("Regeneration left stale state");

        Invoke(window, "ApplyClipboardText", payload);

        original = preview.Source;
        foreach (bool dark in new[] { false, true })
        {
            AppTheme.Apply(window, dark);
            if (preview.Source != original || content.Text != payload) throw new Exception("Theme changed QR data");
            Render(window, $"artifacts/main-{dark}.png", 480, 780);
            var advanced = new AdvancedWindow(new()); AppTheme.Apply(advanced, dark); Render(advanced, $"artifacts/advanced-{dark}.png", 440, 680);
            var template = new TemplateWindow(); AppTheme.Apply(template, dark); Render(template, $"artifacts/templates-{dark}.png", 420, 560);
            var about = new AboutWindow(); AppTheme.Apply(about, dark); Render(about, $"artifacts/about-{dark}.png", 380, 550);
            advanced.Close(); template.Close(); about.Close();
        }
        window.Close();
        File.Delete(path);
        Console.WriteLine("PASS: exact export presets and decoding, dense preset error, Unicode imports and failures, stale/reverted previews, light/dark views.");
    }
    static void Render(Window window, string path, int width, int height)
    {
        var element = (FrameworkElement)window.Content;
        element.Measure(new Size(width, height)); element.Arrange(new Rect(0, 0, width, height)); element.UpdateLayout();
        var bitmap = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32); bitmap.Render(element);
        var encoder = new PngBitmapEncoder(); encoder.Frames.Add(BitmapFrame.Create(bitmap));
        using var output = File.Create(path); encoder.Save(output);
    }
}
