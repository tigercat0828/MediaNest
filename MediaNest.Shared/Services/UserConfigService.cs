using MediaNest.Shared.Entities;
using Microsoft.AspNetCore.Components.Authorization;
using MongoDB.Driver;
using System.Threading.Tasks;
namespace MediaNest.Shared.Services;
public class UserConfigService(
    IMongoCollection<UserConfig> _userConfigs,
    FileService _fileService,
    AuthenticationStateProvider _authStateProvider) : BaseService(_authStateProvider) {

    private const string BackgroundImageFolder = "Background";
    private UserConfig _currentUserConfig = null;

    private FilterDefinition<UserConfig> UserFilter(string username) => Builders<UserConfig>.Filter.Eq(c => c.Username, username);
    public async Task<UserConfig> GetCurrentUserConfig(){
        string username = await GetCurrentUsernameAsync();
        if (username == null) return null;
        var config = await _userConfigs.Find(UserFilter(username)).FirstOrDefaultAsync();
        if (config == null)
        {
            _currentUserConfig = await CreateConfig(username);
        }
         return config;
    }
    public async Task UpdateConfig(UserConfig updatedConfig) {
        string username = await GetCurrentUsernameAsync();
        if (username == null) return;
        updatedConfig.Username = username;
        var filter = UserFilter(username);
        await _userConfigs.ReplaceOneAsync(filter, updatedConfig);
    }
    public async Task<List<EntityCollection>> GetMenuPinnedCollection() {
        
        if(_currentUserConfig == null) await GetCurrentUserConfig();
        return _currentUserConfig.MenuPinnedCollections;
    }
    public async Task<List<EntityCollection>> GetHomePinnedCollection(string username)
    {
        if (_currentUserConfig == null) await GetCurrentUserConfig();
        return _currentUserConfig.HomePinnedCollections;
    }
    // ===================================================================================
    public async Task UploadBackgroundImage(string fileName)
    {
        // 1. 處理檔案移動
        string username = await GetCurrentUsernameAsync();
        if (username == null) return;
        string sourcePath = Path.Combine(_fileService.TaskFolder, fileName);
        string userFolder = Path.Combine(_fileService.UserConfigFolder, username, BackgroundImageFolder);

        if (!Directory.Exists(userFolder)) Directory.CreateDirectory(userFolder);

        string destinationPath = Path.Combine(userFolder, fileName);
        if (File.Exists(sourcePath))
            File.Move(sourcePath, destinationPath, overwrite: true);

        try
        {
            var update = Builders<UserConfig>.Update.AddToSet(c => c.BackgroundImageUrls, fileName);
            await _userConfigs.UpdateOneAsync(UserFilter(username), update, new UpdateOptions { IsUpsert = true });
        }
        catch (Exception ex)
        {
            if (File.Exists(destinationPath)) File.Delete(destinationPath);
            throw;
        }
    }
    public async Task DeleteBackgroundImage(string url) {
        string username = await GetCurrentUsernameAsync();
        if (username == null) return;
        // 1. 從資料庫移除 URL
        var update = Builders<UserConfig>.Update.Pull(c => c.BackgroundImageUrls, url);
        await _userConfigs.UpdateOneAsync(UserFilter(username), update);

        // 2. 嘗試從硬碟刪除檔案 (需從 URL 反推檔案名稱)
        try {
            string fileName = Path.GetFileName(url);
            string filePath = Path.Combine(_fileService.UserConfigFolder, username, BackgroundImageFolder, fileName);
            if (File.Exists(filePath)) File.Delete(filePath);
        }
        catch {
            // Log error or ignore
        }
    }
    private async Task<UserConfig> CreateConfig(string username)
    {

        var newConfig = new UserConfig
        {
            Username = username,
            HomeTitle = "Welcome to MediaNest",
            BackgroundImageUrls = [],
            CommonTags = [],
            MenuPinnedCollections = [],
            HomePinnedCollections = []
        };
        await _userConfigs.InsertOneAsync(newConfig);
        return newConfig;
    }

}