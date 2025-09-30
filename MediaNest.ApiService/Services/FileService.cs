using MongoDB.Bson;

namespace MediaNest.ApiService.Services {
    public class FileService(IConfiguration config) {
        public string AssetsFolder { get; private set; } = Path.GetFullPath(config["AssetsFolder"] ?? "/app/Assets");
        public string ComicFolder => Path.Combine(AssetsFolder, "Comics");
        public string VideoFolder => Path.Combine(AssetsFolder, "Videos");
        public string MusicFolder => Path.Combine(AssetsFolder, "Musics");

        public string TempFolder => Path.Combine(AssetsFolder, "Temp");
        public void SetAssetsFolder(string path) {
            AssetsFolder = path;
        }
        public void DeleteFolder(string path, bool recursive= true) {
            try {
                if (Directory.Exists(path)) {
                    Console.WriteLine("Delete by file service");
                    Directory.Delete(path, recursive);
                }
            }
            catch { 
                // ignore
            }
        }
        public void CreateFolder(string path) {
             Directory.CreateDirectory(path);
        }
        public void CopyFile(string source, string destination, bool overwrite = true) {
            File.Copy(source, destination, overwrite);
        }
        
    }
}

