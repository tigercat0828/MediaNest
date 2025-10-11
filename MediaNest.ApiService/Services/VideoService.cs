using MediaNest.Shared.Entities;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Text.RegularExpressions;

namespace MediaNest.ApiService.Services;

public class VideoService(IMongoCollection<Video> _videoCollection, FileService _fileService) {
    public async Task<int> GetCount() {
        return (int)await _videoCollection.CountDocumentsAsync(_ => true);
    }
    public async Task<List<Video>> GetVideos(int page, int count) {
        return await _videoCollection
            .Find(_ => true)
            .SortByDescending(c => c.Id)
            .Skip((page - 1) * count)
            .Limit(count)
            .ToListAsync();
    }
    public async Task<Video> GetVideoById(string id) {
        return await _videoCollection.Find(video => video.Id == id).FirstOrDefaultAsync();
    }
    public async Task CreateVideo(Video video) {
        await _videoCollection.InsertOneAsync(video);
    }
    public async Task UpdateVideo(string id, Video video) {
        await _videoCollection.ReplaceOneAsync(x => x.Id == id, video);
    }
    public async Task<List<Video>> SearchVideo(string term) {
        var escapedTerm = Regex.Escape(term);

        // 模糊搜尋（加上 .* 讓字串前後可以有其他文字）
        var pattern = $".*{escapedTerm}.*";

        // 建立各種欄位的 regex filter
        var titleFilter = Builders<Video>.Filter.Regex("Title", new BsonRegularExpression(pattern, "i"));
        var codeFilter = Builders<Video>.Filter.Eq("Code", term);
        var tagsFilter = Builders<Video>.Filter.AnyIn("Tags", [term]);

        var combinedFilter = titleFilter | tagsFilter | codeFilter;
        return await _videoCollection.Find(combinedFilter).ToListAsync();
    }
    public async Task DeleteVideo(string id) {
        var video = await GetVideoById(id);
        var folder = Path.Combine(_fileService.VideoFolder, video.Id);
        _fileService.DeleteFolder(folder);
        await _videoCollection.DeleteOneAsync(video => video.Id == id);
    }
}
