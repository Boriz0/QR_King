using System.IO;
using System.Text;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using QRCoder;
using ZXing;

namespace QR_King;

public sealed record GeneratedQr(BitmapSource Image, string Svg, string Text, int Squares);

public static class QrImages
{
    public static BitmapSource Generate(string text) => Create(text, new QrOptions()).Image;

    public static GeneratedQr Create(string text, QrOptions options)
    {
        options.Validate();
        using var generator = new QRCodeGenerator();
        using var data = generator.CreateQrCode(text, Enum.Parse<QRCodeGenerator.ECCLevel>(options.ErrorCorrection), requestedVersion: options.Version == 0 ? -1 : options.Version);
        // QRCoder's matrix includes four quiet-zone squares on each edge.
        int core = data.ModuleMatrix.Count - 8;
        int squares = core + 2 * options.Border;
        int scale = options.ExportSize > 0 ? options.ExportSize / squares : options.PixelsPerModule == 0 ? Math.Max(2, 512 / squares) : options.PixelsPerModule;
        if (scale < 2) throw new ArgumentException("This QR code is too dense for that export size. Choose a larger preset or Auto in Advanced.");
        int size = options.ExportSize > 0 ? options.ExportSize : squares * scale;
        int padding = (size - squares * scale) / 2;
        var fg = QrOptions.ParseColor(options.Foreground);
        var bg = QrOptions.ParseColor(options.Background);
        byte[] pixels = new byte[size * size * 3];
        for (int i = 0; i < pixels.Length; i += 3) { pixels[i] = bg.R; pixels[i + 1] = bg.G; pixels[i + 2] = bg.B; }
        var path = new StringBuilder();
        for (int y = 0; y < core; y++)
            for (int x = 0; x < core; x++)
            {
                if (!data.ModuleMatrix[y + 4][x + 4]) continue;
                int px = x + options.Border, py = y + options.Border;
                path.Append($"M{px} {py}h1v1h-1z");
                for (int dy = 0; dy < scale; dy++)
                    for (int dx = 0; dx < scale; dx++)
                    {
                        int i = ((padding + py * scale + dy) * size + padding + px * scale + dx) * 3;
                        pixels[i] = fg.R; pixels[i + 1] = fg.G; pixels[i + 2] = fg.B;
                    }
            }
        var image = BitmapSource.Create(size, size, 96, 96, PixelFormats.Rgb24, null, pixels, size * 3);
        image.Freeze();
        Verify(image, text);
        string svg = $"<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"{size}\" height=\"{size}\" viewBox=\"0 0 {squares} {squares}\" shape-rendering=\"crispEdges\"><rect width=\"100%\" height=\"100%\" fill=\"{options.Background}\"/><path d=\"{path}\" fill=\"{options.Foreground}\"/></svg>";
        return new GeneratedQr(image, svg, text, squares);
    }

    public static void Verify(BitmapSource image, string text)
    {
        var rgb = new FormatConvertedBitmap(image, PixelFormats.Rgb24, null, 0);
        byte[] pixels = new byte[rgb.PixelWidth * rgb.PixelHeight * 3];
        rgb.CopyPixels(pixels, rgb.PixelWidth * 3, 0);
        var result = new BarcodeReaderGeneric().Decode(pixels, rgb.PixelWidth, rgb.PixelHeight, RGBLuminanceSource.BitmapFormat.RGB24);
        if (result?.Text != text) throw new InvalidOperationException("The QR code did not pass scan verification. Try automatic sizing, black on white, or a shorter text.");
    }

    public static byte[] ToJpeg(BitmapSource image) => Encode(image, new JpegBitmapEncoder { QualityLevel = 100 });
    public static byte[] ToPng(BitmapSource image) => Encode(image, new PngBitmapEncoder());
    private static byte[] Encode(BitmapSource image, BitmapEncoder encoder)
    {
        encoder.Frames.Add(BitmapFrame.Create(image));
        using var stream = new MemoryStream();
        encoder.Save(stream);
        return stream.ToArray();
    }
    public static byte[] Export(GeneratedQr qr, string format)
    {
        if (format == "SVG") return Encoding.UTF8.GetBytes(qr.Svg);
        byte[] bytes = format switch { "PNG" => ToPng(qr.Image), "JPG" => ToJpeg(qr.Image), _ => throw new ArgumentException("Unsupported export format.") };
        using var stream = new MemoryStream(bytes);
        Verify(BitmapDecoder.Create(stream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad).Frames[0], qr.Text);
        return bytes;
    }
}
