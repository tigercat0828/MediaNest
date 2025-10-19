// See https://aka.ms/new-console-template for more information
using System.IO.Compression;
using System.Text;

Console.OutputEncoding = Encoding.UTF8;
Console.Write("目的地: ");
string dstPath = Console.ReadLine();

Directory.CreateDirectory(dstPath);

Console.Write("zipFile: ");
string file = Console.ReadLine();
ComicZipImporter importer = new();
await importer.Unzip(file, dstPath);



class ComicZipImporter {
    public async Task Unzip(string zipFile, string dstPath) {

        string filename = Path.GetFileNameWithoutExtension(zipFile);
        string extractFolder = Path.Combine(dstPath, filename);
        Directory.CreateDirectory(extractFolder);
        // unzip file
        try {
            ZipFile.ExtractToDirectory(zipFile, extractFolder, overwriteFiles: true);
            Console.WriteLine($"✅ 解壓完成：{dstPath}");

            var allFiles = Directory.GetFiles(extractFolder, "*", SearchOption.AllDirectories);
            Console.WriteLine("📂 已解壓的檔案列表：");
            foreach (var file in allFiles) {
                Console.WriteLine("  " + Path.GetRelativePath(extractFolder, file));
            }

            Console.WriteLine($"\n🟢 共 {allFiles.Length} 個檔案");

        }
        catch (Exception ex) {
            Console.WriteLine($"❌ 解壓失敗：{ex.Message}");
        }
    }
}