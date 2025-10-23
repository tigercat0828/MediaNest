using Microsoft.Extensions.Configuration;

namespace MediaNest.Shared.Services;

public class FileService {
    public string AssetsFolder { get; private set; }
    public string ComicFolder => Path.Combine(AssetsFolder, "Comics");
    public string VideoFolder => Path.Combine(AssetsFolder, "Videos");
    public string MusicFolder => Path.Combine(AssetsFolder, "Musics");
    public string ImageFolder => Path.Combine(AssetsFolder, "Images");
    public string TaskFolder => Path.Combine(AssetsFolder, "Task");

    public FileService(IConfiguration _configuration) {
        AssetsFolder = Path.GetFullPath(_configuration["AssetsFolder"] ?? "/app/Assets");
        CreateFolder(TaskFolder);
        CreateFolder(ComicFolder);
        CreateFolder(MusicFolder);
        CreateFolder(VideoFolder);
        CreateFolder(ImageFolder);

    }

    public void FixLongFileNamesInTaskFolder() {
        const int MaxLength = 200; // Linux 單一路徑元件上限為 255 bytes，留安全餘裕

        foreach (string file in Directory.GetFiles(TaskFolder)) {
            string fileName = Path.GetFileName(file);
            string ext = Path.GetExtension(file);
            string nameWithoutExt = Path.GetFileNameWithoutExtension(file);
            
            // 檢查名稱長度
            if (fileName.Length > MaxLength) {
                // 截斷主檔名，保留副檔名
                string truncated = nameWithoutExt[..200];
                string newName = truncated + ext;
                string newPath = Path.Combine(TaskFolder, newName);
                File.Move(file, newPath);
            }
        }
    }
    public void ClearTaskFolder() {
        if (!Directory.Exists(TaskFolder))
            return;

        // 刪除所有檔案
        foreach (string file in Directory.GetFiles(TaskFolder)) {
            File.Delete(file);
        }

        // 刪除所有子資料夾（包含其內部檔案）
        foreach (string dir in Directory.GetDirectories(TaskFolder)) {
            Directory.Delete(dir, recursive: true);
        }
    }
    public void DeleteEmptyAssetFolder() {
        deleteEmptyFolder(ComicFolder);
    }
    private void deleteEmptyFolder(string rootFolder) {
        if (!Directory.Exists(rootFolder))
            return;

        foreach (var dir in Directory.GetDirectories(rootFolder)) {
            // 先遞迴處理子資料夾
            deleteEmptyFolder(dir);

            // 若該資料夾內沒有任何檔案或子資料夾 → 刪除
            if (!Directory.EnumerateFileSystemEntries(dir).Any()) {
                Directory.Delete(dir);
                Console.WriteLine($"🗑️ Delete: {dir}");
            }
        }
    }
    public void SetAssetsFolder(string path) {
        AssetsFolder = path;
    }
    public void DeleteFolder(string path, bool recursive = true) {
        try {
            if (!Directory.Exists(path)) return;
            Directory.Delete(path, recursive);
        }
        catch {
            // ignore
        }
    }
    public void DeleteFile(string file) {
        try {
            if (!File.Exists(file)) return;
            File.Delete(file);
        }
        catch {
            // ignore
        }
    }
    public void CreateFolder(string path) {
        Directory.CreateDirectory(path);
    }
    public void MoveFile(string srcPath, string destPath) {
        File.Move(srcPath, destPath);
    }
    public List<string> ListEntries(string path) {
        var paths = Directory.EnumerateFileSystemEntries(path);
        return [.. paths.Select(Path.GetFileName)];
    }
}
