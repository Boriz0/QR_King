using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Windows.Input;
using Microsoft.Win32;

namespace QR_King;

public partial class MainWindow : Window
{
    private readonly DispatcherTimer timer = new() { Interval = TimeSpan.FromMilliseconds(120) };
    private HwndSource? source;
    private BitmapSource? image;
    private uint ownSequence;
    private bool hasCopied;
    private bool listening;
    private int retries;
    private GeneratedQr? generated;
    private UserPreferences preferences = new();
    private readonly bool persistPreferences;
    private bool loadingPreferences;

    public MainWindow() : this(true) { }

    public MainWindow(bool persistPreferences)
    {
        this.persistPreferences = persistPreferences;
        AppTheme.Attach(this);
        InitializeComponent();
        timer.Tick += ReadClipboard;
        PreviewKeyDown += HandleShortcut;
        if (persistPreferences) preferences = UserPreferences.Load();
        loadingPreferences = true;
        LiveUpdates.IsChecked = preferences.LiveUpdates;
        FitWindow.IsChecked = preferences.FitWindow;
        PinWindow.IsChecked = preferences.AlwaysOnTop;
        Topmost = preferences.AlwaysOnTop;
        var area = SystemParameters.WorkArea;
        Width = Math.Clamp(double.IsFinite(preferences.Width) ? preferences.Width : 480, Math.Min(MinWidth, area.Width), area.Width);
        Height = Math.Clamp(double.IsFinite(preferences.Height) ? preferences.Height : 780, Math.Min(MinHeight, area.Height), area.Height);
        if (preferences.Left is double left && preferences.Top is double top && double.IsFinite(left) && double.IsFinite(top))
        {
            WindowStartupLocation = WindowStartupLocation.Manual;
            Left = left; Top = top;
        }
        loadingPreferences = false;
        if (!preferences.LiveUpdates) StatusText.Text = "Updates paused. Edit the text, then press Regenerate.";
    }

    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);
        if (preferences.LoadWarning != null)
            MessageBox.Show(this, preferences.LoadWarning, "Settings cleanup", MessageBoxButton.OK, MessageBoxImage.Warning);
        source = HwndSource.FromHwnd(new WindowInteropHelper(this).Handle);
        ConstrainToMonitor(false);
        source.AddHook(WindowMessage);
        listening = AddClipboardFormatListener(source.Handle);
        if (!listening)
            MessageBox.Show(this, "Clipboard monitoring could not start. Restart QR King to try again.", "QR King", MessageBoxButton.OK, MessageBoxImage.Warning);
        ScheduleRead();
    }

    private IntPtr WindowMessage(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (msg == 0x031D) ScheduleRead();
        return IntPtr.Zero;
    }

    private void ScheduleRead()
    {
        if (LiveUpdates.IsChecked != true) return;
        retries = 0;
        timer.Stop();
        timer.Start();
    }

    private void LiveUpdates_Changed(object sender, RoutedEventArgs e)
    {
        if (!IsInitialized || loadingPreferences) return;
        timer.Stop();
        if (LiveUpdates.IsChecked == true)
        {
            StatusText.Text = "Listening for clipboard changes";
            ScheduleRead();
        }
        else StatusText.Text = "Updates paused. Edit the text, then press Regenerate.";
        SavePreferences();
    }

    private void ContentText_Changed(object sender, System.Windows.Controls.TextChangedEventArgs e)
    {
        if (!IsInitialized || LiveUpdates.IsChecked == true) return;
        bool stale = generated != null && ContentText.Text != generated.Text;
        StaleOverlay.Visibility = stale ? Visibility.Visible : Visibility.Collapsed;
        SaveButton.IsEnabled = CopyButton.IsEnabled = generated != null && !stale;
        ExportDimensions.Text = stale ? "Export size: regenerate to update" : generated != null ? DescribeExport(generated) : "Export size: generate a QR code first";
        ErrorText.Visibility = Visibility.Collapsed;
        StatusText.Text = generated != null && !stale ? "Preview matches the current text · Manual" : "Text edited. Press Regenerate to update the QR code.";
    }

    private void Regenerate_Click(object sender, RoutedEventArgs e)
    {
        if (LiveUpdates.IsChecked == true) return;
        GenerateFromText(ContentText.Text);
    }

    private void ReadClipboard(object? sender, EventArgs e)
    {
        timer.Stop();
        if (LiveUpdates.IsChecked != true) return;
        if (hasCopied && GetClipboardSequenceNumber() == ownSequence) return;
        try
        {
            ApplyClipboardText(Clipboard.ContainsText() ? Clipboard.GetText() : null);
        }
        catch (COMException)
        {
            if (++retries < 6) timer.Start();
            else ShowClipboardError("Cannot read the clipboard. Another app may be using it. Wait a moment, then copy your text again.");
        }
    }

    private void ApplyClipboardText(string? text)
    {
        ContentText.Text = text ?? string.Empty;
        GenerateFromText(text);
    }

    private void GenerateFromText(string? text)
    {
        if (string.IsNullOrEmpty(text))
        {
            ShowClipboardError(LiveUpdates.IsChecked == true
                ? "The clipboard is empty or contains unsupported content. Copy text or a URL instead of an image or file."
                : "There is no text to encode. Enter text or a URL, then press Regenerate.");
            return;
        }
        if (text.Length > 10000)
        {
            ShowTooLongError();
            return;
        }
        try
        {
            generated = QrImages.Create(text, preferences.Qr);
            SetImage(generated.Image);
            if (FitWindow.IsChecked == true) ConstrainToMonitor(true);
            ErrorText.Visibility = Visibility.Collapsed;
            ErrorText.Text = string.Empty;
            StatusText.Text = $"{text.Length:N0} characters · Scan verified · {(LiveUpdates.IsChecked == true ? "Live" : "Manual")}";
        }
        catch (QRCoder.Exceptions.DataTooLongException)
        {
            ShowTooLongError();
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
        {
            ShowClipboardError(ex.Message);
        }
    }

    private void ShowTooLongError() => ShowClipboardError(LiveUpdates.IsChecked == true
        ? "This text is too long for the selected QR settings. Copy shorter text, or choose Auto version / lower error correction in Advanced."
        : "This text is too long for the selected QR settings. Shorten it or choose Auto version / lower error correction, then press Regenerate.");

    private void ShowClipboardError(string message)
    {
        SetImage(null);
        EmptyHint.Text = "Cannot generate a QR code";
        ErrorText.Text = "Error: " + message;
        ErrorText.Visibility = Visibility.Visible;
        StatusText.Text = LiveUpdates.IsChecked == true ? "Waiting for compatible clipboard text." : "Live updates off. Edit the text to try again.";
    }

    private void SetImage(BitmapSource? value)
    {
        image = value;
        if (value == null) generated = null;
        QrImage.Source = value;
        StaleOverlay.Visibility = Visibility.Collapsed;
        ExportDimensions.Text = value != null && generated != null ? DescribeExport(generated) : "Export size: generate a QR code first";
        EmptyHint.Visibility = value == null ? Visibility.Visible : Visibility.Collapsed;
        SaveButton.IsEnabled = CopyButton.IsEnabled = value != null;
    }

    private static string DescribeExport(GeneratedQr qr) => $"Export: {qr.Image.PixelWidth:N0} × {qr.Image.PixelHeight:N0} px · SVG is scalable";

    private void SaveImage_Click(object sender, RoutedEventArgs e)
    {
        if (generated == null || !SaveButton.IsEnabled) return;
        var snapshot = generated;
        string[] formats = ["PNG", "JPG", "SVG"];
        var dialog = new SaveFileDialog { Filter = "PNG image (*.png)|*.png|JPEG image (*.jpg)|*.jpg|SVG vector (*.svg)|*.svg", FilterIndex = Array.IndexOf(formats, preferences.Qr.Format) + 1, DefaultExt = "." + preferences.Qr.Format.ToLowerInvariant(), FileName = "QR-King", AddExtension = true };
        if (dialog.ShowDialog(this) != true) return;
        try
        {
            string format = formats[dialog.FilterIndex - 1];
            File.WriteAllBytes(dialog.FileName, QrImages.Export(snapshot, format));
            preferences.Qr.Format = format;
            SavePreferences();
            StatusText.Text = "Image saved.";
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or InvalidOperationException)
        {
            MessageBox.Show(this, $"Could not save the image.\n\n{ex.Message}", "QR King", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private void CopyImage_Click(object sender, RoutedEventArgs e)
    {
        if (image == null || generated == null || !CopyButton.IsEnabled) return;
        try
        {
            using var jpeg = new MemoryStream(QrImages.Export(generated, "JPG"));
            using var png = new MemoryStream(QrImages.Export(generated, "PNG"));
            var data = new DataObject();
            data.SetImage(image);
            data.SetData("JFIF", jpeg);
            data.SetData("PNG", png);
            Clipboard.SetDataObject(data, true);
            ownSequence = GetClipboardSequenceNumber();
            hasCopied = true;
            StatusText.Text = "Image copied. Paste it into an image-compatible app.";
        }
        catch (COMException)
        {
            StatusText.Text = "Clipboard is busy. Please try Copy image again.";
        }
        catch (InvalidOperationException ex) { ShowClipboardError(ex.Message); }
    }

    private void About_Click(object sender, RoutedEventArgs e) => new AboutWindow { Owner = this }.ShowDialog();

    protected override void OnClosed(EventArgs e)
    {
        SavePreferences();
        timer.Stop();
        if (source != null)
        {
            if (listening) RemoveClipboardFormatListener(source.Handle);
            source.RemoveHook(WindowMessage);
        }
        base.OnClosed(e);
    }

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool AddClipboardFormatListener(IntPtr hwnd);
    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool RemoveClipboardFormatListener(IntPtr hwnd);
    [DllImport("user32.dll")]
    private static extern uint GetClipboardSequenceNumber();
}
