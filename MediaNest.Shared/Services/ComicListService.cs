using MediaNest.Shared.Entities;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Text.RegularExpressions;

namespace MediaNest.Shared.Services;

public class ComicListService(IMongoCollection<ComicList> _listCollection) {
    public async Task CreateComicList(List<string> comicIds, string title) {
        ComicList list = new() {
            Title = title,
            ComicIds = comicIds
        };
        await _listCollection.InsertOneAsync(list);
    }
    public async Task CreateComicList(ComicList list) {
        await _listCollection.InsertOneAsync(list);
    }
    public async Task<List<ComicList>> SearchComicList(string term) {
        var escapedTerm = Regex.Escape(term);

        // 模糊搜尋（加上 .* 讓字串前後可以有其他文字）
        var pattern = $".*{escapedTerm}.*";

        // 建立各種欄位的 regex filter
        var titleFilter = Builders<ComicList>.Filter.Regex("Title", new BsonRegularExpression(pattern, "i"));

        // 建立 SearchComicByTags 的 Filter
        var tagsFilter = Builders<ComicList>.Filter.AnyIn("Tags", [term]);

        var combinedFilter = titleFilter | tagsFilter;
        return await _listCollection.Find(combinedFilter).ToListAsync();
    }
    public async Task<int> GetCount() {
        return (int)await _listCollection.CountDocumentsAsync(_ => true);
    }
    public async Task<List<ComicList>> GetRamdomComicLists(int count) {
        return await _listCollection.Aggregate().Sample(count).ToListAsync();
    }
    public async Task<ComicList> GetComicListById(string id) {
        return await _listCollection.Find(list => list.Id == id).FirstOrDefaultAsync();
    }
    public async Task<List<ComicList>> GetComicLists(int page, int count) {

        return await _listCollection
            .Find(_ => true)
            .SortByDescending(c => c.Id)
            .Skip((page - 1) * count)
            .Limit(count)
            .ToListAsync();
    }
    public async Task UpdateComicList(string id, ComicList list) {
        await _listCollection.ReplaceOneAsync(x => x.Id == id, list);
    }
    public async Task DeleteComicList(string id) {
        await _listCollection.DeleteOneAsync(list => list.Id == id);
    }
    public async Task<bool> CheckExists(string id) {
        return await _listCollection.Find(list => list.Id == id).AnyAsync();
    }

}