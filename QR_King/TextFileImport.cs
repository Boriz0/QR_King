using System.IO;
using System.Text;
using System.Windows;

namespace QR_King;

public static class TextFileImport
{
    public const int MaximumBytes = 64 * 1024;
    public static string Read(string path)
    {
        if (!Path.GetExtension(path).Equals(".txt", StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("Drop one .txt file. Other file types are not supported.");
        using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        byte[] bytes = new byte[MaximumBytes + 1];
        int length = 0, read;
        while (length < bytes.Length && (read = stream.Read(bytes, length, bytes.Length - length)) > 0) length += read;
        if (length > MaximumBytes) throw new ArgumentException("This file is too large. Drop a .txt file of 64 KB or less, or copy a shorter excerpt.");
        Encoding encoding = new UTF8Encoding(false, true);
        int skip = 0;
        if (length >= 4 && bytes[0] == 0xff && bytes[1] == 0xfe && bytes[2] == 0 && bytes[3] == 0) { encoding = new UTF32Encoding(false, true, true); skip = 4; }
        else if (length >= 4 && bytes[0] == 0 && bytes[1] == 0 && bytes[2] == 0xfe && bytes[3] == 0xff) { encoding = new UTF32Encoding(true, true, true); skip = 4; }
        else if (length >= 3 && bytes[0] == 0xef && bytes[1] == 0xbb && bytes[2] == 0xbf) skip = 3;
        else if (length >= 2 && bytes[0] == 0xff && bytes[1] == 0xfe) { encoding = new UnicodeEncoding(false, true, true); skip = 2; }
        else if (length >= 2 && bytes[0] == 0xfe && bytes[1] == 0xff) { encoding = new UnicodeEncoding(true, true, true); skip = 2; }
        string text;
        try { text = encoding.GetString(bytes, skip, length - skip); }
        catch (DecoderFallbackException) { throw new ArgumentException("This file is not valid Unicode text. Save it as UTF-8 and try again."); }
        if (text.Any(c => char.IsControl(c) && c != '\r' && c != '\n' && c != '\t'))
            throw new ArgumentException("This file appears to contain binary data. Save plain text as UTF-8 and try again.");
        return text;
    }
}

public partial class MainWindow
{
    private void Content_DragOver(object sender, DragEventArgs e)
    {
        e.Handled = true;
        e.Effects = DragDropEffects.None;
        if ((e.AllowedEffects & DragDropEffects.Copy) != 0 && e.Data.GetData(DataFormats.FileDrop) is string[] paths && paths.Length == 1 &&
            Path.GetExtension(paths[0]).Equals(".txt", StringComparison.OrdinalIgnoreCase)) e.Effects = DragDropEffects.Copy;
    }

    private void Content_Drop(object sender, DragEventArgs e)
    {
        e.Handled = true;
        if (e.Data.GetData(DataFormats.FileDrop) is not string[] paths || paths.Length != 1)
        {
            ShowImportError("Drop one .txt file at a time.");
            return;
        }
        ImportTextFile(paths[0]);
    }

    private void ImportTextFile(string path)
    {
        try
        {
            string text = TextFileImport.Read(path);
            LiveUpdates.IsChecked = false;
            timer.Stop();
            ApplyClipboardText(text);
        }
        catch (Exception ex) when (ex is ArgumentException or IOException or UnauthorizedAccessException)
        {
            ShowImportError(ex is ArgumentException ? ex.Message : "Could not read that file. Check its permissions and try again.");
        }
    }

    private void ShowImportError(string message)
    {
        ErrorText.Text = message;
        ErrorText.Visibility = Visibility.Visible;
    }
}
