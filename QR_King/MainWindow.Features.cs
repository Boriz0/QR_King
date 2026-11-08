using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;

namespace QR_King;

public partial class MainWindow
{
    private void HandleShortcut(object sender, KeyEventArgs e)
    {
        if (Keyboard.Modifiers != ModifierKeys.Control) return;
        if (e.Key == Key.Enter && LiveUpdates.IsChecked != true) { Regenerate_Click(this, new RoutedEventArgs()); e.Handled = true; }
        if (e.Key == Key.S && SaveButton.IsEnabled) { SaveImage_Click(this, new RoutedEventArgs()); e.Handled = true; }
    }

    private void Preferences_Changed(object sender, RoutedEventArgs e)
    {
        if (!IsInitialized || loadingPreferences) return;
        Topmost = PinWindow.IsChecked == true;
        if (FitWindow.IsChecked == true && generated != null) ConstrainToMonitor(true);
        SavePreferences();
    }

    private void Advanced_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new AdvancedWindow(preferences.Qr) { Owner = this };
        if (dialog.ShowDialog() != true) return;
        preferences.Qr = dialog.Options;
        if (!string.IsNullOrEmpty(ContentText.Text)) GenerateFromText(ContentText.Text);
        SavePreferences();
    }

    private void Templates_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new TemplateWindow { Owner = this };
        if (dialog.ShowDialog() != true) return;
        LiveUpdates.IsChecked = false;
        ApplyClipboardText(dialog.Payload);
    }

    private void Clear_Click(object sender, RoutedEventArgs e)
    {
        timer.Stop();
        ContentText.Clear();
        SetImage(null);
        ErrorText.Text = "";
        ErrorText.Visibility = Visibility.Collapsed;
        EmptyHint.Text = "Waiting for text";
        StatusText.Text = LiveUpdates.IsChecked == true ? "Cleared. Copy new text to generate a QR code." : "Cleared. Enter text, then press Regenerate.";
    }

    private void SavePreferences()
    {
        if (!persistPreferences || loadingPreferences || !IsInitialized) return;
        preferences.LiveUpdates = LiveUpdates.IsChecked == true;
        preferences.FitWindow = FitWindow.IsChecked == true;
        preferences.AlwaysOnTop = PinWindow.IsChecked == true;
        Rect bounds = WindowState == WindowState.Normal ? new Rect(Left, Top, Width, Height) : RestoreBounds;
        if (double.IsFinite(bounds.Left) && double.IsFinite(bounds.Top))
        {
            preferences.Left = bounds.Left; preferences.Top = bounds.Top;
            preferences.Width = bounds.Width; preferences.Height = bounds.Height;
        }
        try { preferences.Save(); }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            StatusText.Text = "Could not save preferences. Check access to the local settings folder.";
        }
    }

    private void ConstrainToMonitor(bool fit)
    {
        var area = SystemParameters.WorkArea;
        IntPtr handle = new WindowInteropHelper(this).Handle;
        var info = new MonitorInfo { Size = Marshal.SizeOf<MonitorInfo>() };
        if (handle != IntPtr.Zero && GetMonitorInfo(MonitorFromWindow(handle, 2), ref info))
        {
            var matrix = source?.CompositionTarget?.TransformFromDevice ?? Matrix.Identity;
            var start = matrix.Transform(new Point(info.Work.Left, info.Work.Top));
            var end = matrix.Transform(new Point(info.Work.Right, info.Work.Bottom));
            area = new Rect(start, end);
        }
        MinWidth = Math.Min(440, area.Width); MinHeight = Math.Min(680, area.Height);
        if (WindowState != WindowState.Normal) return;
        if (fit && generated != null)
        {
            // Four display pixels per square keeps dense codes readable; exports are independent.
            double side = Math.Max(280, generated.Squares * 4);
            Width = Math.Clamp(side + 74, MinWidth, area.Width);
            UpdateLayout();
            double surroundingHeight = ActualHeight > 0 && QrImage.ActualHeight > 0 ? ActualHeight - QrImage.ActualHeight : 470;
            Height = Math.Clamp(side + surroundingHeight, MinHeight, area.Height);
        }
        else { Width = Math.Min(Width, area.Width); Height = Math.Min(Height, area.Height); }
        if (double.IsFinite(Left)) Left = Math.Clamp(Left, area.Left, area.Right - Width);
        if (double.IsFinite(Top)) Top = Math.Clamp(Top, area.Top, area.Bottom - Height);
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct NativeRect { public int Left, Top, Right, Bottom; }
    [StructLayout(LayoutKind.Sequential)]
    private struct MonitorInfo { public int Size; public NativeRect Monitor, Work; public uint Flags; }
    [DllImport("user32.dll")]
    private static extern IntPtr MonitorFromWindow(IntPtr hwnd, uint flags);
    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetMonitorInfo(IntPtr monitor, ref MonitorInfo info);
}
