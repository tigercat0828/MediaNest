using Microsoft.Extensions.Configuration;

namespace MediaNest.Shared.Services;

public class FileService(IConfiguration _configuration) {
    public string AssetsFolder { get; private set; } = Path.GetFullPath(_configuration["AssetsFolder"] ?? "/app/Assets");
    public string ComicFolder => Path.Combine(AssetsFolder, "Comics");
    public string VideoFolder => Path.Combine(AssetsFolder, "Videos");
    public string MusicFolder => Path.Combine(AssetsFolder, "Musics");
    public string ImageFolder => Path.Combine(AssetsFolder, "Images");
    public string TaskFolder => Path.Combine(AssetsFolder, "Task");
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
