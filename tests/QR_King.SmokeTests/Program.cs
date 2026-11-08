using System.IO;
using System.Reflection;
using System.Windows.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using QR_King;
using ZXing;

internal static class Program
{
    [STAThread]
    static void Main()
    {
        string[] samples = ["https://example.com", "Boris Vacula", "Žilina — Bratislava 😀\nSecond line", "  preserve whitespace  ", new string('x', 1000)];
        foreach (string text in samples)
        {
            var image = QrImages.Generate(text);
            VerifyDecode(image, text);
            byte[] jpeg = QrImages.ToJpeg(image);
            if (jpeg[0] != 0xff || jpeg[1] != 0xd8) throw new Exception("Not JPEG");
            using var stream = new MemoryStream(jpeg);
            var decoded = BitmapDecoder.Create(stream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad).Frames[0];
            VerifyDecode(decoded, text);
        }
        try { QrImages.Generate(new string('x', 10000)); throw new Exception("Oversized input accepted"); }
        catch (QRCoder.Exceptions.DataTooLongException) { }

        FeatureTests.Run();
        InteractionTests.Run();
        var window = new MainWindow(false);
        var apply = typeof(MainWindow).GetMethod("ApplyClipboardText", BindingFlags.Instance | BindingFlags.NonPublic)!;
        var live = (CheckBox)window.FindName("LiveUpdates");
        var error = (TextBlock)window.FindName("ErrorText");
        var timer = (DispatcherTimer)typeof(MainWindow).GetField("timer", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(window)!;
        foreach (string? invalid in new string?[] { null, "", new string('x', 5000), new string('x', 10001) })
        {
            apply.Invoke(window, [invalid]);
            if (error.Visibility != Visibility.Visible || !error.Text.Contains("Copy", StringComparison.OrdinalIgnoreCase)
                || ((Button)window.FindName("SaveButton")).IsEnabled || ((Image)window.FindName("QrImage")).Source != null)
                throw new Exception("Invalid clipboard content did not show actionable error");
        }
        apply.Invoke(window, [samples[0]]);
        if (error.Visibility != Visibility.Collapsed || !((Button)window.FindName("SaveButton")).IsEnabled)
            throw new Exception("Valid content did not recover from error");
        var originalImage = ((Image)window.FindName("QrImage")).Source;
        live.IsChecked = false;
        typeof(MainWindow).GetMethod("ScheduleRead", BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(window, null);
        typeof(MainWindow).GetMethod("ReadClipboard", BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(window, [null, EventArgs.Empty]);
        if (timer.IsEnabled || ((Image)window.FindName("QrImage")).Source != originalImage)
            throw new Exception("Paused updates did not preserve preview");
        live.IsChecked = true;
        if (!timer.IsEnabled) throw new Exception("Resuming did not schedule clipboard refresh");
        live.IsChecked = false;
        if (timer.IsEnabled) throw new Exception("Pausing did not cancel pending refresh");
        var content = (TextBox)window.FindName("ContentText");
        var regenerate = (Button)window.FindName("RegenerateButton");
        if (content.IsReadOnly || regenerate.Visibility != Visibility.Visible)
            throw new Exception("Manual editing controls unavailable");
        string edited = "Edited text: Žilina 😀\nAnother line";
        content.Text = edited;
        if (((Image)window.FindName("QrImage")).Source != originalImage || ((Button)window.FindName("SaveButton")).IsEnabled)
            throw new Exception("Draft should wait for regeneration and prevent stale export");
        regenerate.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        VerifyDecode((BitmapSource)((Image)window.FindName("QrImage")).Source, edited);
        foreach (string invalid in new[] { "", new string('x', 5000) })
        {
            content.Text = invalid;
            regenerate.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            if (error.Visibility != Visibility.Visible || !error.Text.Contains("Regenerate"))
                throw new Exception("Manual error must explain how to recover");
        }
        content.Text = edited;
        regenerate.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        if (error.Visibility != Visibility.Collapsed || timer.IsEnabled) throw new Exception("Manual recovery failed");
        live.IsChecked = true;
        if (!content.IsReadOnly || regenerate.Visibility != Visibility.Collapsed) throw new Exception("Live mode must prevent manual editing");
        live.IsChecked = false;
        var grid = (Grid)window.Content;
        apply.Invoke(window, [samples[0]]);
        grid.Measure(new Size(460, 700));
        grid.Arrange(new Rect(0, 0, 460, 700));
        grid.UpdateLayout();
        var preview = new RenderTargetBitmap(460, 700, 96, 96, PixelFormats.Pbgra32);
        preview.Render(grid);
        var encoder = new PngBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(preview));
        using var file = File.Create("tests/preview.png");
        encoder.Save(file);
        var about = new AboutWindow();
        if (!((TextBlock)about.FindName("VersionText")).Text.Contains("0.2.1")) throw new Exception("About version missing");
        Console.WriteLine("PASS: QR/JPEG round trips, invalid-content errors, pause/resume, manual edits and regeneration, manual error recovery, mode controls, main and About views.");
    }

    static void VerifyDecode(BitmapSource image, string expected)
    {
        var rgb = new FormatConvertedBitmap(image, PixelFormats.Rgb24, null, 0);
        byte[] pixels = new byte[rgb.PixelWidth * rgb.PixelHeight * 3];
        rgb.CopyPixels(pixels, rgb.PixelWidth * 3, 0);
        var result = new BarcodeReaderGeneric().Decode(pixels, rgb.PixelWidth, rgb.PixelHeight, RGBLuminanceSource.BitmapFormat.RGB24);
        if (result?.Text != expected) throw new Exception("QR payload did not round-trip");
    }
}
