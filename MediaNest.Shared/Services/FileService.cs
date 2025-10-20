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
}
