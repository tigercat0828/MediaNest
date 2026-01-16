using MediaNest.Shared.Entities;
using MediaNest.Shared.Models;
using Microsoft.AspNetCore.Components.Authorization;

namespace MediaNest.Shared.Services;


public class ComicService(
    EntityRepository<Comic> _comicRepo,
    EntityRepository<ComicList> _listRepo,
    FileService _fileService,
    AuthenticationStateProvider authStateProvider) : BaseService(authStateProvider) {
    // entitiy
    public async Task<List<Comic>> SearchComic(string term) => await _comicRepo.Search(term);
    public async Task<int> GetComicCount() => await _comicRepo.GetCount();
    public async Task<Comic> GetComicById(string id) => await _comicRepo.GetById(id);
    public async Task<List<Comic>> GetComics(int page, int count) => await _comicRepo.GetByPage(page, count);
    public async Task<List<Comic>> GetRandomComics(int count) => await _comicRepo.GetRandom(count);
    public async Task UpdateComic(string id, Comic updated, string oldTitle) {
        if (!await AuthorizeAsync(UserRole.User)) return;
        string newImgPath = Path.Combine(_fileService.ComicFolder, updated.Folder);
        string oldImgPath = Path.Combine(_fileService.ComicFolder, $"{updated.Code[..3]}", $"[{updated.Code}]{oldTitle}");

        string newThumbPath = Path.Combine(_fileService.ComicFolder, "Thumbs", updated.Folder);
        string oldThumbPath = Path.Combine(_fileService.ComicFolder, "Thumbs", $"{updated.Code[..3]}", $"[{updated.Code}]{oldTitle}");
        if (oldImgPath != newImgPath) Directory.Move(oldImgPath, newImgPath);
        if (oldThumbPath != newThumbPath && Directory.Exists(oldThumbPath)) Directory.Move(oldThumbPath, newThumbPath);
        await _comicRepo.Update(id, updated);

    }
    public async Task UpdateComicWithoutFileOperation(string id, Comic updated) {
        if (!await AuthorizeAsync(UserRole.User)) return;
        await _comicRepo.Update(id, updated);
    }
    public async Task DeleteComic(string id) {
        if (!await AuthorizeAsync(UserRole.User)) return;
        var comic = await GetComicById(id);
        var sourcefolder = Path.Combine(_fileService.ComicFolder, comic.Folder);
        var thumbsFolder = Path.Combine(_fileService.ComicFolder, "Thumbs", comic.Folder);
        _fileService.DeleteFolder(sourcefolder);
        _fileService.DeleteFolder(thumbsFolder);
        await _comicRepo.Delete(id);
    }
    public async Task CreateComic(Comic comic, bool fileOp = true) {
        if (!await AuthorizeAsync(UserRole.User)) return;
        await _comicRepo.Create(comic);
        if (fileOp) {
            var resizer = new ImageResizer();
            string srcFolder = Path.Combine(_fileService.ComicFolder, comic.Folder);
            string dstFolder = Path.Combine(_fileService.ComicFolder, "Thumbs", comic.Folder);
            resizer.ResizeImageInFolder(srcFolder, dstFolder, 0.2f);
        }
    }
    public async Task SplitComic(Comic comic) {
        if (!await AuthorizeAsync(UserRole.User)) return;
        if (comic.Bookmarks.Count < 2) return;

        List<string> comicIds = [];

        string sourceFolder = Path.Combine(_fileService.ComicFolder, comic.Folder);
        string thumbFolder = Path.Combine(_fileService.ComicFolder, "Thumbs", comic.Folder);

        // 一次取所有檔案並依頁碼排序
        var allFiles = Directory.GetFiles(sourceFolder).OrderBy(ExtractPageNumber).ToList();
        var allThumbs = Directory.GetFiles(thumbFolder).OrderBy(ExtractPageNumber).ToList();



        // bookmark = [1, 9, 27, 36]
        for (int i = 0; i < comic.Bookmarks.Count; i++) {
            int start = comic.Bookmarks[i];
            int end = (i < comic.Bookmarks.Count - 1) ? comic.Bookmarks[i + 1] - 1 : int.MaxValue;

            Comic subComic = new() {
                Title = $"{comic.Title} {i + 1}",
                SubTitle = $"{comic.SubTitle} {i + 1}",
                Author = comic.Author,
                Series = comic.Series,
                Uploader = comic.Uploader,
                Code = Utility.GenerateSixDigitCode()
            };

            // 插入 DB，driver 自動回填 Id
            await _comicRepo.Create(subComic);

            // 新資料夾位置
            string targetFolder = Path.Combine(_fileService.ComicFolder, subComic.Folder);
            string targetThumbFolder = Path.Combine(_fileService.ComicFolder, "Thumbs", subComic.Folder);
            Directory.CreateDirectory(targetFolder);
            Directory.CreateDirectory(targetThumbFolder);

            // 搬移並重新命名檔案
            var images = allFiles.Where(f => IsPageInRange(f, start, end)).ToList();
            int pageNo = 1;
            foreach (var imgPath in images) {
                string ext = Path.GetExtension(imgPath);
                string newName = $"{pageNo:D3}{ext}";
                string destPath = Path.Combine(targetFolder, newName);
                File.Copy(imgPath, destPath, overwrite: true);
                pageNo++;
            }

            // 搬移並重新命名縮圖（若存在）
            var thumbs = allThumbs.Where(f => IsPageInRange(f, start, end)).ToList();
            int thumbNo = 1;
            foreach (var thumbPath in thumbs) {
                string ext = Path.GetExtension(thumbPath);
                string newName = $"{thumbNo:D3}{ext}";
                string destPath = Path.Combine(targetThumbFolder, newName);
                File.Copy(thumbPath, destPath, overwrite: true);
                thumbNo++;
            }

            // 記錄 Comic Id
            comicIds.Add(subComic.Id);

            Console.WriteLine($"SplitComic triggered for {subComic.Title} at {DateTime.Now}");
        }
        ComicList list = new() {
            ComicIds = comicIds,
            Title = comic.Title
        };
        // 存 ComicList
        await _listRepo.Create(list);

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
    // list
    public async Task<List<ComicList>> SearchList(string term) => await _listRepo.Search(term);
    public async Task<List<ComicList>> GetLists(int page, int count) => await _listRepo.GetByPage(page, count);
    public async Task<int> GetListCount() => await _listRepo.GetCount();
    public async Task<ComicList> GetListById(string id) => await _listRepo.GetById(id);
    public async Task<List<ComicList>> GetRandomLists(int count) => await _listRepo.GetRandom(count);
    public async Task DeleteList(string id) {
        if (!await AuthorizeAsync(UserRole.User)) return;
        await _listRepo.Delete(id);
    }
    public async Task UpdateList(string id, ComicList updated) {
        if (!await AuthorizeAsync(UserRole.User)) return;
        await _listRepo.Update(id, updated);
    }
    public async Task CreateList(ComicList list) {
        if (!await AuthorizeAsync(UserRole.User)) return;
        await _listRepo.Create(list);
    }
}