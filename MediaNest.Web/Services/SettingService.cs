using System.Text.Json;

namespace MediaNest.Web.Services; 
public class SettingService {


    private readonly string _configPath;

    public SettingService(IWebHostEnvironment env) {
        _configPath = Path.Combine(env.ContentRootPath, "appsettings.json");
    }
    public async Task<string?> GetAssetsFolderAsync() {
        if (!File.Exists(_configPath)) return null;

        var json = await File.ReadAllTextAsync(_configPath);
        using var doc = JsonDocument.Parse(json);
        if (doc.RootElement.TryGetProperty("AssetsFolder", out var elem)) {
            return elem.GetString();
        }
        return null;
    }

    public async Task SetAssetsFolderAsync(string newPath) {
        var json = await File.ReadAllTextAsync(_configPath);
        var dict = JsonSerializer.Deserialize<Dictionary<string, object>>(json) ?? [];

        dict["AssetsFolder"] = newPath;

        string newJson = JsonSerializer.Serialize(dict, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(_configPath, newJson);
    }

}
