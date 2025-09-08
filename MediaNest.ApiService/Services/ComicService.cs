using MediaNest.Shared.Entities;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Net.NetworkInformation;

namespace MediaNest.ApiService.Services {
    public class ComicService(IMongoCollection<Comic> collection) {
        public async Task<int> GetCount() {
            return (int)await collection.CountDocumentsAsync(_ => true);
        }
        public async Task<List<Comic>> GetAllComics() {
            return await collection.Find(_ => true).ToListAsync();
        }
        public async Task<List<Comic>> GetRandomComics(int count) {
            return await collection.Aggregate().Sample(count).ToListAsync();
        }
        public async Task<List<Comic>> GetComics(int page, int count) {

            return await collection
                .Find(_ => true)
                .Skip((page - 1) * count)
                .Limit(count)
                .ToListAsync();
        }
        public async Task<Comic> GetComicById(string id) {
            return await collection.Find(comic => comic.Id == id).FirstOrDefaultAsync();
        }
        public async Task DeleteComic(string id) {
            // Todo : delete file here           
            await collection.DeleteOneAsync(comic => comic.Id == id);
        }
        public async Task CreateComic(Comic comic) {
            await collection.InsertOneAsync(comic);
        }
        public async Task UpdateComic(string id, Comic comic) {
            await collection.ReplaceOneAsync(x => x.Id == id, comic);
        }
        public async Task<List<Comic>> SearchComic(string term) {
            // 建立 SearchComicByIdInfo 的 Filter
            var titleFilter = Builders<Comic>.Filter.Regex("Title", new BsonRegularExpression(term, "i"));
            var sourceTitleFilter = Builders<Comic>.Filter.Regex("SourceTitle", new BsonRegularExpression(term, "i"));
            var authorFilter = Builders<Comic>.Filter.Regex("Author", new BsonRegularExpression(term, "i"));
            var parodyFilter = Builders<Comic>.Filter.Regex("Series", new BsonRegularExpression(term, "i"));
            var codeFilter = Builders<Comic>.Filter.Eq("Code", term);

            // 建立 SearchComicByTags 的 Filter
            var tagsFilter = Builders<Comic>.Filter.AnyIn("Tags", [term]);
            var charactersFilter = Builders<Comic>.Filter.AnyIn("Characters", [term]);

            var combinedFilter = titleFilter | sourceTitleFilter | authorFilter | parodyFilter | codeFilter | tagsFilter | charactersFilter;
            return await collection.Find(combinedFilter).ToListAsync();
        }
        public async Task<bool> CheckExists(string id) {
            return await collection.Find(comic => comic.Id == id).AnyAsync();
        }
    }
}
