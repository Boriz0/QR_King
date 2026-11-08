using Microsoft.Win32;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;

namespace QR_King;

public static class AppTheme
{
    public static void Attach(Window window)
    {
        window.Resources.MergedDictionaries.Add(new ResourceDictionary { Source = new Uri("/QR_King;component/Theme.xaml", UriKind.Relative) });
        window.SetResourceReference(Window.BackgroundProperty, "SurfaceBrush");
        window.SetResourceReference(Window.ForegroundProperty, "TextBrush");
        Apply(window);
        HwndSource? source = null;
        IntPtr Hook(IntPtr hwnd, int message, IntPtr w, IntPtr l, ref bool handled)
        {
            if (message is 0x001A or 0x031A) window.Dispatcher.BeginInvoke(() => Apply(window));
            return IntPtr.Zero;
        }
        window.SourceInitialized += (_, _) =>
        {
            source = HwndSource.FromHwnd(new WindowInteropHelper(window).Handle);
            source?.AddHook(Hook);
            Apply(window);
        };
        window.Closed += (_, _) => source?.RemoveHook(Hook);
    }

    public static void Apply(Window window, bool? darkOverride = null)
    {
        bool dark = darkOverride ?? IsDark();
        bool contrast = SystemParameters.HighContrast && darkOverride == null;
        void Brush(string key, string light, string night, Color system)
            => window.Resources[key] = new SolidColorBrush(contrast ? system : (Color)ColorConverter.ConvertFromString(dark ? night : light));
        Brush("SurfaceBrush", "#F6F7F9", "#202020", SystemColors.WindowColor);
        Brush("FieldBrush", "#FFFFFF", "#303030", SystemColors.WindowColor);
        Brush("TextBrush", "#202124", "#F3F3F3", SystemColors.WindowTextColor);
        Brush("MutedBrush", "#596273", "#BCC3CE", SystemColors.GrayTextColor);
        Brush("BorderBrush", "#B8BEC8", "#747474", SystemColors.WindowTextColor);
        Brush("HoverBrush", "#E2E8F0", "#454545", SystemColors.HighlightColor);
        Brush("AccentBrush", "#005FB8", "#75BCFF", SystemColors.HighlightColor);
        Brush("ErrorBrush", "#B42318", "#FFACA5", SystemColors.WindowTextColor);
        var handle = new WindowInteropHelper(window).Handle;
        if (handle != IntPtr.Zero)
        {
            int value = dark && !contrast ? 1 : 0;
            DwmSetWindowAttribute(handle, 20, ref value, sizeof(int));
        }
    }

    private static bool IsDark()
    {
        try { return Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize", "AppsUseLightTheme", 1) is int value && value == 0; }
        catch (Exception ex) when (ex is System.Security.SecurityException or UnauthorizedAccessException or System.IO.IOException) { return false; }
    }

    [DllImport("dwmapi.dll")]
    private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attribute, ref int value, int size);
}
