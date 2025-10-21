using MediaNest.Shared.Entities;
using System.Globalization;
using System.IO.Compression;
using System.Text;
using System.Text.RegularExpressions;

namespace MediaNest.Shared.Models;

public class ComicZipImporter(string dstFolder) {
    private readonly ImageResizer _resizer = new();
    private readonly Regex _pattern = new(@"\[(.*?)\](.*)", RegexOptions.Compiled);
    private readonly string ComicFolder = dstFolder;

    public Comic ImportComic(string zipFile) {
        string filename = Path.GetFileNameWithoutExtension(zipFile);
        (string author, string title) = GetAuthorAndTitle(filename);
        Comic comic = new() {
            Author = author,
            Title = title
        };
        string imgFolder = Path.Combine(ComicFolder, comic.Folder);
        // unzip file
        Unzip(zipFile, imgFolder);
        // rename file to page number
        int pageNo = 1;

        var srcFiles = Directory.GetFiles(imgFolder)
                                .OrderBy(f => Path.GetFileName(f), StringComparer.Create(CultureInfo.CurrentCulture, false))
                                .ToList();

        foreach (var srcFile in srcFiles) {
            string file = Path.GetFileNameWithoutExtension(srcFile);
            string ext = Path.GetExtension(srcFile);
            string newName = $"{pageNo++:D3}{ext}";
            string newPath = Path.Combine(imgFolder, newName);
            File.Move(srcFile, newPath, true);
        }
        // generate thumbs
        string thumbFolder = Path.Combine(ComicFolder, "Thumbs", comic.Folder);
        _resizer.ResizeImageInFolder(imgFolder, thumbFolder, 0.2f);

        return comic;
    }
    public (string Author, string Title) GetAuthorAndTitle(string name) {
        // [Author] Title
        var match = _pattern.Match(name);
        if (match.Success) {
            string author = match.Groups[1].Value.Trim();
            string title = match.Groups[2].Value.Trim();
            return (author, title);
        }
        // No [Author] found
        return (string.Empty, name.Trim());
    }
    public void Unzip(string zipFile, string dstPath) {

        Directory.CreateDirectory(dstPath);
        // unzip file
        try {
            // ZipFile.ExtractToDirectory(zipFile, dstPath, overwriteFiles: true);

            using var fs = new FileStream(zipFile, FileMode.Open, FileAccess.Read);
            using var archive = new ZipArchive(fs, ZipArchiveMode.Read, false, Encoding.GetEncoding("UTF-8"));

            int count = 0;
            foreach (var entry in archive.Entries) {
                // 只處理檔案，略過資料夾 entry
                if (string.IsNullOrEmpty(entry.Name))
                    continue;

                // Console.WriteLine($" unzip : {entry.FullName}");

                string destinationPath = Path.Combine(dstPath, entry.FullName);
                string? dir = Path.GetDirectoryName(destinationPath);
                if (!string.IsNullOrEmpty(dir))
                    Directory.CreateDirectory(dir);

                try {
                    using var entryStream = entry.Open();
                    using var outputStream = File.Create(destinationPath);
                    entryStream.CopyTo(outputStream);
                    count++;
                }
                catch (Exception ex) {
                    Console.WriteLine($"⚠️ 跳過 {entry.FullName}：{ex.Message}");
                }
            }
            Console.WriteLine($"✅ 解壓完成 ({count} 個檔案)：{dstPath}");
        }
        catch (Exception ex) {
            Console.WriteLine($"❌ 解壓失敗：{ex.Message}");
        }
    }
    public List<string> GetAllZipFiles(string folderPath) {
        if (!Directory.Exists(folderPath))
            return [];

        // 搜尋所有 .zip 檔案（包含子資料夾）
        return [.. Directory.EnumerateFiles(folderPath, "*.zip", SearchOption.AllDirectories)];
    }
}