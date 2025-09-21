using MediaNest.Shared;
using MediaNest.Shared.Entities;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Net.NetworkInformation;

namespace MediaNest.Web.Services {
    public class ComicService(IMongoCollection<Comic> _comicCollection, IMongoCollection<ComicList> _listCollection) {
        public async Task<int> GetCount() {
            return (int)await _comicCollection.CountDocumentsAsync(_ => true);
        }
        public async Task<List<Comic>> GetAllComics() {
            return await _comicCollection.Find(_ => true).ToListAsync();
        }
        public async Task<List<Comic>> GetRandomComics(int count) {
            return await _comicCollection.Aggregate().Sample(count).ToListAsync();
        }
        public async Task<List<Comic>> GetComics(int page, int count) {

            return await _comicCollection
                .Find(_ => true)
                .Skip((page - 1) * count)
                .Limit(count)
                .ToListAsync();
        }
        public async Task<Comic> GetComicById(string id) {
            return await _comicCollection.Find(comic => comic.Id == id).FirstOrDefaultAsync();
        }
        public async Task DeleteComic(string id) {
            // Todo : delete file here           
            await _comicCollection.DeleteOneAsync(comic => comic.Id == id);
        }
        public async Task CreateComic(Comic comic) {
            await _comicCollection.InsertOneAsync(comic);
        }
        public async Task UpdateComic(string id, Comic comic) {
            await _comicCollection.ReplaceOneAsync(x => x.Id == id, comic);
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
            return await _comicCollection.Find(combinedFilter).ToListAsync();
        }
        public async Task<bool> CheckExists(string id) {
            return await _comicCollection.Find(comic => comic.Id == id).AnyAsync();
        }
        public async Task SplitComic(Comic comic) {
            if (comic.Bookmarks.Count < 2) return;

            ComicList list = new() { Title = comic.Title };

            int count = 1;
            string sourceFolder = Path.Combine(AppState.AssetsFolder, "Comics", comic.FolderName);

            // bookmark = [1, 9, 27, 36]
            for (int i = 0; i < comic.Bookmarks.Count; i++) {
                int start = comic.Bookmarks[i];
                int end = (i < comic.Bookmarks.Count - 1) ? comic.Bookmarks[i + 1] - 1 : int.MaxValue;

                // 建立子 Comic（不必設定 FolderName，會自動算）
                Comic subComic = new() {
                    Title = $"{comic.Title} {count++}",
                    SubTitle = $"{comic.SubTitle} {count++}",
                    Author = comic.Author,
                    Series = comic.Series,
                    Uploader = comic.Uploader,
                    Code = Utility.GenerateSixDigitCode()
                };

                // 插入 DB，driver 自動回填 Id
                await CreateComic(subComic);

                // 新資料夾位置
                string targetFolder = Path.Combine(AppState.AssetsFolder, "Comics", subComic.FolderName);
                Directory.CreateDirectory(targetFolder);

                // 搬移並重新命名檔案
                var images = Directory.GetFiles(sourceFolder)
                    .Where(f => IsPageInRange(f, start, end))
                    .OrderBy(f => f)
                    .ToList();

                int pageNo = 1;
                foreach (var imgPath in images) {
                    string ext = Path.GetExtension(imgPath);
                    string newName = $"{pageNo:D3}{ext}";
                    string destPath = Path.Combine(targetFolder, newName);
                    File.Copy(imgPath, destPath, overwrite:true);
                    pageNo++;
                }

                // 記錄 Comic Id
                list.ComicIds.Add(subComic.Id);
            }

            // 存 ComicList
            await _listCollection.InsertOneAsync(list);
            
        }

        private bool IsPageInRange(string filePath, int start, int end) {
            string fileName = Path.GetFileNameWithoutExtension(filePath);
            if (int.TryParse(fileName, out int pageNum)) {
                return pageNum >= start && pageNum <= end;
            }
            return false;
        }

    }
}
