using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
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

    public MainWindow()
    {
        InitializeComponent();
        timer.Tick += ReadClipboard;
    }

    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);
        source = HwndSource.FromHwnd(new WindowInteropHelper(this).Handle);
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
        retries = 0;
        timer.Stop();
        if (LiveUpdates.IsChecked == true) timer.Start();
    }

    private void LiveUpdates_Changed(object sender, RoutedEventArgs e)
    {
        if (!IsInitialized) return;
        timer.Stop();
        if (LiveUpdates.IsChecked == true)
        {
            StatusText.Text = "Listening for clipboard changes";
            ScheduleRead();
        }
        else StatusText.Text = "Updates paused. Edit the text, then press Regenerate.";
    }

    private void ContentText_Changed(object sender, System.Windows.Controls.TextChangedEventArgs e)
    {
        if (!IsInitialized || LiveUpdates.IsChecked == true) return;
        SaveButton.IsEnabled = CopyButton.IsEnabled = false;
        ErrorText.Visibility = Visibility.Collapsed;
        StatusText.Text = "Text edited. Press Regenerate to update the QR code.";
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
            SetImage(QrImages.Generate(text));
            ErrorText.Visibility = Visibility.Collapsed;
            ErrorText.Text = string.Empty;
            StatusText.Text = $"{text.Length:N0} characters · {(LiveUpdates.IsChecked == true ? "Updates automatically" : "QR code regenerated · Live updates off")}";
        }
        catch (QRCoder.Exceptions.DataTooLongException)
        {
            ShowTooLongError();
        }
    }

    private void ShowTooLongError() => ShowClipboardError(LiveUpdates.IsChecked == true
        ? "This text is too long for a single QR code. Copy a shorter excerpt or a shorter URL, then try again."
        : "This text is too long for a single QR code. Shorten the text or URL, then press Regenerate.");

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
        QrImage.Source = value;
        EmptyHint.Visibility = value == null ? Visibility.Visible : Visibility.Collapsed;
        SaveButton.IsEnabled = CopyButton.IsEnabled = value != null;
    }

    private void SaveImage_Click(object sender, RoutedEventArgs e)
    {
        if (image == null) return;
        var snapshot = image;
        var dialog = new SaveFileDialog { Filter = "JPEG image (*.jpg)|*.jpg", DefaultExt = ".jpg", FileName = "QR-King.jpg", AddExtension = true };
        if (dialog.ShowDialog(this) != true) return;
        try
        {
            File.WriteAllBytes(dialog.FileName, QrImages.ToJpeg(snapshot));
            StatusText.Text = "Image saved.";
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            MessageBox.Show(this, $"Could not save the image.\n\n{ex.Message}", "QR King", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private void CopyImage_Click(object sender, RoutedEventArgs e)
    {
        if (image == null) return;
        try
        {
            using var jpeg = new MemoryStream(QrImages.ToJpeg(image));
            var data = new DataObject();
            data.SetImage(image);
            data.SetData("JFIF", jpeg);
            Clipboard.SetDataObject(data, true);
            ownSequence = GetClipboardSequenceNumber();
            hasCopied = true;
            StatusText.Text = "Image copied. Paste it into an image-compatible app.";
        }
        catch (COMException)
        {
            StatusText.Text = "Clipboard is busy. Please try Copy image again.";
        }
    }

    private void About_Click(object sender, RoutedEventArgs e) => new AboutWindow { Owner = this }.ShowDialog();

    protected override void OnClosed(EventArgs e)
    {
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
