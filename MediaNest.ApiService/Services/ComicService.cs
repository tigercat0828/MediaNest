using MediaNest.Shared;
using MediaNest.Shared.Entities;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Text.RegularExpressions;

namespace MediaNest.ApiService.Services;

public class ComicService(FileService _fileService, IMongoCollection<Comic> _comicCollection, IMongoCollection<ComicList> _listCollection) {
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
            .SortByDescending(c => c.Id)
            .Skip((page - 1) * count)
            .Limit(count)
            .ToListAsync();
    }
    public async Task<Comic> GetComicById(string id) {
        return await _comicCollection.Find(comic => comic.Id == id).FirstOrDefaultAsync();
    }
    public async Task DeleteComic(string id) {
        var comic = await GetComicById(id);
        var folder = Path.Combine(_fileService.ComicFolder, comic.FolderName);
        _fileService.DeleteFolder(folder);
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
        var escapedTerm = Regex.Escape(term);

        // 模糊搜尋（加上 .* 讓字串前後可以有其他文字）
        var pattern = $".*{escapedTerm}.*";

        // 建立各種欄位的 regex filter
        var titleFilter = Builders<Comic>.Filter.Regex("Title", new BsonRegularExpression(pattern, "i"));
        var subTitleFilter = Builders<Comic>.Filter.Regex("SubTitle", new BsonRegularExpression(pattern, "i"));
        var authorFilter = Builders<Comic>.Filter.Regex("Author", new BsonRegularExpression(pattern, "i"));
        var seriesFilter = Builders<Comic>.Filter.Regex("Series", new BsonRegularExpression(pattern, "i"));

        var codeFilter = Builders<Comic>.Filter.Eq("Code", term);

        // 建立 SearchComicByTags 的 Filter
        var tagsFilter = Builders<Comic>.Filter.AnyIn("Tags", [term]);
        var charactersFilter = Builders<Comic>.Filter.AnyIn("Characters", [term]);


        var combinedFilter = titleFilter | subTitleFilter | authorFilter | seriesFilter | codeFilter | tagsFilter | charactersFilter;
        return await _comicCollection.Find(combinedFilter).ToListAsync();
    }
    public async Task<bool> CheckExists(string id) {
        return await _comicCollection.Find(comic => comic.Id == id).AnyAsync();
    }
    public async Task SplitComic(Comic comic) {
        if (comic.Bookmarks.Count < 2) return;

        List<string> comicIds = [];

        string sourceFolder = Path.Combine(_fileService.ComicFolder, comic.FolderName);

        // 一次取所有檔案並依頁碼排序
        var allFiles = Directory.GetFiles(sourceFolder)
            .OrderBy(f => ExtractPageNumber(f))
            .ToList();

        // bookmark = [1, 9, 27, 36]
        for (int i = 0; i < comic.Bookmarks.Count; i++) {
            int start = comic.Bookmarks[i];
            int end = (i < comic.Bookmarks.Count - 1) ? comic.Bookmarks[i + 1] - 1 : int.MaxValue;

            // 建立子 Comic（不必設定 FolderName，會自動算）
            Comic subComic = new() {
                Title = $"{comic.Title} {i + 1}",
                SubTitle = $"{comic.SubTitle} {i + 1}",
                Author = comic.Author,
                Series = comic.Series,
                Uploader = comic.Uploader,
                Code = Utility.GenerateSixDigitCode()
            };

            // 插入 DB，driver 自動回填 Id
            await CreateComic(subComic);

            // 新資料夾位置
            string targetFolder = Path.Combine(_fileService.ComicFolder, subComic.FolderName);
            Directory.CreateDirectory(targetFolder);

            // 搬移並重新命名檔案
            var images = allFiles
                .Where(f => IsPageInRange(f, start, end))
                .ToList();

            int pageNo = 1;
            foreach (var imgPath in images) {
                string ext = Path.GetExtension(imgPath);
                string newName = $"{pageNo:D3}{ext}";
                string destPath = Path.Combine(targetFolder, newName);
                File.Copy(imgPath, destPath, overwrite: true);
                pageNo++;
            }

            // 記錄 Comic Id
            comicIds.Add(subComic.Id);

            Console.WriteLine($"SplitComic triggered for {subComic.Title} at {DateTime.Now}");
        }

        // 存 ComicList
        await CreateComicList(comicIds, comic.Title);


        int ExtractPageNumber(string path) {
            string fileName = Path.GetFileNameWithoutExtension(path);
            if (int.TryParse(fileName, out int num)) return num;
            return int.MaxValue; // 無法解析的排到最後
        }

        bool IsPageInRange(string filePath, int start, int end) {
            // 001.jpg => 001
            string fileName = Path.GetFileNameWithoutExtension(filePath);
            if (int.TryParse(fileName, out int pageNum)) {
                return pageNum >= start && pageNum <= end;
            }
            return false;
        }
    }
   
    public async Task CreateComicList(List<string> comicIds, string title) {
        ComicList list = new() {
            Title = title,
            ComicIds = comicIds
        };
        await _listCollection.InsertOneAsync(list);
    }
}
