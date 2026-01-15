using SkiaSharp;

namespace MediaNest.Shared.Models;

public class ImageResizer {
    private static readonly string[] SupportedExtensions = [ ".jpg", ".jpeg", ".png", ".webp" ];

    public void ResizeImageInFolder(string srcFolder, string dstFolder, float ratio) {
        var imgFiles = GetImageFiles(srcFolder);
        Console.WriteLine($" Found {imgFiles.Length} files.");
        // ✅ 若資料夾沒有支援的圖片 → 跳過
        if (imgFiles.Length == 0)
            return;

        Directory.CreateDirectory(dstFolder);

        int count = 0;
        foreach (var srcFile in imgFiles) {
            string filename = $"{Path.GetFileNameWithoutExtension(srcFile)}.jpg";
            string dstFile = Path.Combine(dstFolder, filename);

            if (File.Exists(dstFile))
                continue; // 避免重覆壓縮

            try {
                Resize(srcFile, dstFile, ratio);
                count++;
            }
            catch (Exception ex) {
                Console.WriteLine($"❌ {srcFile}: {ex.Message}");
            }
        }

        if (count > 0)
            Console.WriteLine($"📘 {Path.GetFileName(srcFolder)} → ✅ {count} images resized\n");
    }

    private static string[] GetImageFiles(string folder)
        => [.. Directory.GetFiles(folder, "*.*", SearchOption.TopDirectoryOnly).Where(f => SupportedExtensions.Contains(Path.GetExtension(f).ToLowerInvariant()))];

    public void Resize(string source, string destination, float ratio) {
        using var input = File.OpenRead(source);
        using var original = SKBitmap.Decode(input);

        if (original == null)
            throw new Exception("Decode failed (unsupported or corrupted image).");

        int newWidth = (int)(original.Width * ratio);
        int newHeight = (int)(original.Height * ratio);

        using var resizedImage = SKImage.FromBitmap(
            original.Resize(new SKImageInfo(newWidth, newHeight), SKSamplingOptions.Default)
        );

        using var output = File.Open(destination, FileMode.Create, FileAccess.Write);
        resizedImage.Encode(SKEncodedImageFormat.Jpeg, 80).SaveTo(output);
    }
}
