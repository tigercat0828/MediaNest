using SkiaSharp;

class Program {

    static async Task Main() {

        Console.OutputEncoding = System.Text.Encoding.UTF8;

        Console.Write("Enter Comics root folder (ex: Assets/Comics): ");
        string? comicsRoot = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(comicsRoot) || !Directory.Exists(comicsRoot)) {
            Console.WriteLine("❌ Invalid path.");
            return;
        }

        string thumbsRoot = Path.Combine(comicsRoot, "Thumbs");
        Directory.CreateDirectory(thumbsRoot);

        ImageResizer resizer = new();
        await GenerateAllComicsThumbnails(comicsRoot, thumbsRoot, resizer, 0.2f);

        Console.WriteLine("\n🎉 All thumbnails generated!");
    }

    static async Task GenerateAllComicsThumbnails(string comicsRoot, string thumbsRoot, ImageResizer resizer, float ratio) {
        var comicDirs = Directory.EnumerateDirectories(comicsRoot, "*", SearchOption.AllDirectories)
                                 .Where(d => !d.Contains("Thumbs"))
                                 .ToList();

        Console.WriteLine($"🧭 Found {comicDirs.Count} comic folders.\n");

        var options = new ParallelOptions {
            MaxDegreeOfParallelism = Math.Max(1, Environment.ProcessorCount - 1)
        };

        await Parallel.ForEachAsync(comicDirs, options, async (comicDir, _) => {
            try {

                // 建立對應 Thumbs 資料夾（保留目錄階層）
                string relativePath = Path.GetRelativePath(comicsRoot, comicDir);
                string dstFolder = Path.Combine(thumbsRoot, relativePath);

                Console.WriteLine($"📘 {relativePath}");
                resizer.ResizeFolder(comicDir, dstFolder, ratio);
            }
            catch (Exception ex) {
                Console.WriteLine($"⚠️ Error in {comicDir}: {ex.Message}");
            }
        });
    }
}

public class ImageResizer {
    private static readonly string[] SupportedExtensions = { ".jpg", ".jpeg", ".png", ".webp" };

    public void ResizeFolder(string srcFolder, string dstFolder, float ratio) {
        var imgFiles = GetImageFiles(srcFolder);

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
        => Directory.GetFiles(folder, "*.*", SearchOption.TopDirectoryOnly)
                    .Where(f => SupportedExtensions.Contains(Path.GetExtension(f).ToLowerInvariant()))
                    .ToArray();

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
