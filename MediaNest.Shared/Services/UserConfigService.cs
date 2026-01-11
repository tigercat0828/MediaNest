using MediaNest.Shared.Entities;
using MongoDB.Driver;
namespace MediaNest.Shared.Services;

public class UserConfigService(IMongoCollection<UserConfig> _userConfigs, FileService _fileService) {
    private const string BackgroundImageFolder = "Background";
    private FilterDefinition<UserConfig> UserFilter(string username) =>
        Builders<UserConfig>.Filter.Eq(c => c.Username, username);

    public async Task SetHomeTitle(string username, string title) {
        var update = Builders<UserConfig>.Update.Set(c => c.HomeTitle, title);
        await _userConfigs.UpdateOneAsync(UserFilter(username), update, new UpdateOptions { IsUpsert = true });
    }

    public async Task SetCommonTags(string username, List<string> tags) {
        var update = Builders<UserConfig>.Update.Set(c => c.CommonTags, tags);
        await _userConfigs.UpdateOneAsync(UserFilter(username), update, new UpdateOptions { IsUpsert = true });
    }

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
    public async Task EditMenuPinnedCollection(string username, List<EntityCollection> collections) {
        var update = Builders<UserConfig>.Update.Set(c => c.MenuPinnedCollections, collections);
        await _userConfigs.UpdateOneAsync(UserFilter(username), update, new UpdateOptions { IsUpsert = true });
    }
    public async Task EditHomePinnedCollection(string username, List<EntityCollection> collections) {
        var update = Builders<UserConfig>.Update.Set(c => c.HomePinnedCollections, collections);
        await _userConfigs.UpdateOneAsync(UserFilter(username), update, new UpdateOptions { IsUpsert = true });
    }
}