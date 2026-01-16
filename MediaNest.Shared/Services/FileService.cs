using Microsoft.Extensions.Configuration;

namespace MediaNest.Shared.Services;

public class FileService {
    public string AssetsFolder { get; private set; }
    public string UserConfigFolder => Path.Combine(AssetsFolder, "Users");
    public string FigureFolder => Path.Combine(AssetsFolder, "Figures");
    public string ComicFolder => Path.Combine(AssetsFolder, "Comics");
    public string VideoFolder => Path.Combine(AssetsFolder, "Videos");
    public string MusicFolder => Path.Combine(AssetsFolder, "Musics");
    public string ImageFolder => Path.Combine(AssetsFolder, "Images");
    public string TaskFolder => Path.Combine(AssetsFolder, "Task");

    public FileService(IConfiguration _configuration) {
        AssetsFolder = Path.GetFullPath(_configuration["AssetsFolder"] ?? "/app/Assets");
        CreateFolder(UserConfigFolder);
        CreateFolder(TaskFolder);

        CreateFolder(ComicFolder);
        CreateFolder(MusicFolder);
        CreateFolder(VideoFolder);
        CreateFolder(ImageFolder);
        CreateFolder(FigureFolder);
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
    public async Task SaveStreamAsync(Stream srcStream, string destPath, long totalSize, Func<int, Task>? onProgress = null) {
        byte[] buffer = new byte[81920]; // 80KB
        long totalRead = 0;
        int bytesRead;

        await using FileStream fs = new(destPath, FileMode.Create);
        while ((bytesRead = await srcStream.ReadAsync(buffer)) > 0) {
            await fs.WriteAsync(buffer.AsMemory(0, bytesRead));
            totalRead += bytesRead;

            if (onProgress != null && totalSize > 0) {
                int progress = (int)((double)totalRead / totalSize * 100);
                await onProgress(progress);
            }
        }
        
        if (onProgress != null) {
            await onProgress(100);
        }
    }
}
