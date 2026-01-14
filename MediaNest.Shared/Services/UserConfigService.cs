using MediaNest.Shared.Entities;
using MongoDB.Driver;
using System.Threading.Tasks;
namespace MediaNest.Shared.Services;

/*
   TODO 所有public XXXX(string username, ....) 之方法 應該為private作為內部邏輯方法,避免client竄改username傳入
    對外使用AuthenticationStateProvider 取得當前使用者名稱

    ex. 
public class UserConfigService(
    IMongoCollection<UserConfig> _userConfigs, 
    FileService _fileService,
    AuthenticationStateProvider _authStateProvider){

    private async Task<string> GetCurrentUsernameAsync()
    {
        var authState = await _authStateProvider.GetAuthenticationStateAsync();
        var user = authState.User;

        if (user.Identity == null || !user.Identity.IsAuthenticated)
        {
            throw new UnauthorizedAccessException("使用者未登入");
        }

        return user.Identity.Name!;
    }
}
 */
public class UserConfigService(IMongoCollection<UserConfig> _userConfigs, FileService _fileService) {
    private const string BackgroundImageFolder = "Background";
    private UserConfig _currentUserConfig = null;

    private FilterDefinition<UserConfig> UserFilter(string username) => Builders<UserConfig>.Filter.Eq(c => c.Username, username);
    public async Task<UserConfig> GetUserConfig(string username)
    {
        var config = await _userConfigs.Find(UserFilter(username)).FirstOrDefaultAsync();
        if(config == null) await CreateConfig(username);
        return config;
    }
    public async Task<UserConfig> CreateConfig(string username) { 
        
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
    public async Task UpdateConfig(string username, UserConfig updatedConfig) {
        var filter = UserFilter(username);
        await _userConfigs.ReplaceOneAsync(filter, updatedConfig);
    }

    public async Task<List<EntityCollection>> GetMenuPinnedCollection(string username) {
        if(_currentUserConfig == null)
        {
            _currentUserConfig = await GetUserConfig(username);
        }
        return _currentUserConfig.MenuPinnedCollections;
    }
    public async Task<List<EntityCollection>> GetHomePinnedCollection(string username)
    {
        if (_currentUserConfig == null)
        {
            _currentUserConfig = await GetUserConfig(username);
        }
        return _currentUserConfig.HomePinnedCollections;
    }

    // ===================================================================================
    public async Task UploadBackgroundImage(string username, string fileName) {
        // 1. 處理檔案移動
        string sourcePath = Path.Combine(_fileService.TaskFolder, fileName);
        string userFolder = Path.Combine(_fileService.UserConfigFolder, username, BackgroundImageFolder);

        if (!Directory.Exists(userFolder)) Directory.CreateDirectory(userFolder);

        string destinationPath = Path.Combine(userFolder, fileName);
        if (File.Exists(sourcePath)) {
            File.Move(sourcePath, destinationPath, overwrite: true);
        }
        var update = Builders<UserConfig>.Update.AddToSet(c => c.BackgroundImageUrls, fileName);
        await _userConfigs.UpdateOneAsync(UserFilter(username), update, new UpdateOptions { IsUpsert = true });
    }
    public async Task DeleteBackgroundImage(string username, string url) {
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
}