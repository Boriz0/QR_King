using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows.Media;

namespace QR_King;

public sealed class QrOptions
{
    public string ErrorCorrection { get; set; } = "M";
    public int PixelsPerModule { get; set; } // 0 chooses an integer scale near 512 pixels.
    public int ExportSize { get; set; } // 0 = automatic/custom module scale; otherwise exact square canvas.
    public int Border { get; set; } = 4;
    public int Version { get; set; } // 0 = automatic
    public string Foreground { get; set; } = "#000000";
    public string Background { get; set; } = "#FFFFFF";
    public string Format { get; set; } = "PNG";
    public QrOptions Copy() => (QrOptions)MemberwiseClone();
    public void Validate()
    {
        if (!new[] { "L", "M", "Q", "H" }.Contains(ErrorCorrection)) throw new ArgumentException("Choose error correction L, M, Q or H.");
        if (PixelsPerModule != 0 && (PixelsPerModule < 2 || PixelsPerModule > 16)) throw new ArgumentException("Pixels per square must be Auto or 2–16.");
        if (!new[] { 0, 256, 512, 1024, 2048 }.Contains(ExportSize)) throw new ArgumentException("Choose Auto, 256, 512, 1024 or 2048 pixels.");
        if (ExportSize != 0 && PixelsPerModule != 0) throw new ArgumentException("Choose either an export-size preset or custom pixels per square.");
        if (Border < 4 || Border > 16) throw new ArgumentException("The border must be 4–16 QR squares.");
        if (Version < 0 || Version > 40) throw new ArgumentException("QR version must be Auto or 1–40.");
        if (!new[] { "PNG", "JPG", "SVG" }.Contains(Format)) throw new ArgumentException("Choose PNG, JPG or SVG.");
        var fg = ParseColor(Foreground);
        var bg = ParseColor(Background);
        if (Luminance(bg) <= Luminance(fg) || (Luminance(bg) + .05) / (Luminance(fg) + .05) < 4.5)
            throw new ArgumentException("Use a dark foreground and a lighter background with strong contrast (at least 4.5:1).");
    }
    public static Color ParseColor(string value)
    {
        if (value == null || !System.Text.RegularExpressions.Regex.IsMatch(value, "^#[0-9a-fA-F]{6}$"))
            throw new ArgumentException("Enter colours as six-digit hex values, for example #000000.");
        return (Color)ColorConverter.ConvertFromString(value);
    }
    private static double Luminance(Color c)
    {
        static double Linear(byte b) { double v = b / 255.0; return v <= .04045 ? v / 12.92 : Math.Pow((v + .055) / 1.055, 2.4); }
        return .2126 * Linear(c.R) + .7152 * Linear(c.G) + .0722 * Linear(c.B);
    }
}

public sealed class UserPreferences
{
    public QrOptions Qr { get; set; } = new();
    public bool LiveUpdates { get; set; } = true;
    public bool FitWindow { get; set; }
    public bool AlwaysOnTop { get; set; }
    [JsonIgnore]
    public string? LoadWarning { get; private set; }
    public double Width { get; set; } = 480;
    public double Height { get; set; } = 780;
    public double? Left { get; set; }
    public double? Top { get; set; }
    public static string FilePath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "QR King", "settings.json");
    public static UserPreferences Load(string? path = null)
    {
        string destination = path ?? FilePath;
        var prefs = new UserPreferences();
        try
        {
            bool rewrite = File.Exists(destination + ".tmp");
            string? json = null;
            try { json = File.ReadAllText(destination); }
            catch (Exception ex) when (ex is FileNotFoundException or DirectoryNotFoundException) { }
            if (json != null)
            {
                try
                {
                    using var document = JsonDocument.Parse(json);
                    // Upgrade older settings without restoring or retaining their text payload.
                    rewrite |= document.RootElement.ValueKind != JsonValueKind.Object ||
                        document.RootElement.EnumerateObject().Any(p =>
                            p.Name.Equals("SavedText", StringComparison.OrdinalIgnoreCase) ||
                            p.Name.Equals("RememberText", StringComparison.OrdinalIgnoreCase));
                    prefs = JsonSerializer.Deserialize<UserPreferences>(json) ?? new();
                    prefs.Qr ??= new();
                    prefs.Qr.Validate();
                }
                catch (Exception ex) when (ex is JsonException or ArgumentException or InvalidOperationException)
                {
                    prefs = new();
                    rewrite = true;
                }
            }
            // Atomic replacement also overwrites any abandoned temporary settings file.
            if (rewrite) prefs.Save(destination);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            prefs.LoadWarning = "Could not read or clean up the settings file. Old saved text may remain on disk. Close older copies of QR King and remove settings.json and settings.json.tmp from %LOCALAPPDATA%\\QR King, or fix folder permissions and restart.";
        }
        return prefs;
    }
    public void Save(string? path = null)
    {
        string destination = path ?? FilePath;
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(destination))!);
        string temp = destination + ".tmp";
        File.WriteAllText(temp, JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true }));
        File.Move(temp, destination, true);
    }

}
