using MediaNest.Shared;
using MediaNest.Shared.Entities;
using System.Dynamic;
using System.IO.Compression;
using System.Text;
using System.Text.RegularExpressions;



Console.InputEncoding = Encoding.UTF8;
Console.OutputEncoding = Encoding.UTF8;

Directory.CreateDirectory("C:\\Task");

string targetPath = Console.ReadLine();
ComicZipImporter importer = new();
var files = importer.GetAllZipFiles(targetPath);


foreach(var file in files) {
    await importer.ImportComic(file);
}



class ComicZipImporter {

    private readonly Regex _pattern = new(@"\[(.*?)\](.*)", RegexOptions.Compiled);

    public async Task ImportComic(string zipFile) {
        string filename = Path.GetFileNameWithoutExtension(zipFile);
        (string author, string title) = GetAuthorAndTitle(filename);
        Comic comic = new()
        {
            Author = author,
            Title = title
        };
        string dstPath = Path.Combine("C:/Task", comic.Folder);
        await Unzip(zipFile, dstPath);
        // await service

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
    public async Task Unzip(string zipFile, string dstPath) {

        Directory.CreateDirectory(dstPath);
        // unzip file
        try {
            ZipFile.ExtractToDirectory(zipFile, dstPath, overwriteFiles: true);
            Console.WriteLine($"✅ 解壓完成：{dstPath}");

            var allFiles = Directory.GetFiles(dstPath, "*", SearchOption.AllDirectories);
            Console.WriteLine("📂 已解壓的檔案列表：");
            foreach (var file in allFiles) {
                Console.WriteLine("  " + Path.GetRelativePath(dstPath, file));
            }

            Console.WriteLine($"\n🟢 共 {allFiles.Length} 個檔案");

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