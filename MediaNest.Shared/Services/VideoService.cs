using MediaNest.Shared.Entities;
using MediaNest.Shared.Services.Background;
using System.Diagnostics;

namespace MediaNest.Shared.Services;

public class VideoService(EntityRepository<Video> _videoRepo,
                          EntityRepository<VideoList> _listRepo,
                          FileService _fileService,
                          IBackgroundTaskQueue _taskQueue) {
    public async Task<int> GetCount() => await _videoRepo.GetCount();
    public async Task<List<Video>> GetAllVideos() => await _videoRepo.GetAll();
    public async Task<List<Video>> Search(string term) => await _videoRepo.Search(term);
    public async Task<List<Video>> GetPage(int page, int count) => await _videoRepo.GetByPage(page, count);
    public async Task<List<Video>> GetRandomVideos(int count) => await _videoRepo.GetRandom(count);
    public async Task<Video> GetById(string id) => await _videoRepo.GetById(id);
    public async Task CreateVideo(string srcFile) {
        Video video = new() {
            Title = Path.GetFileNameWithoutExtension(srcFile)
        };
        string newFilename = $"{video.Title}.mp4";
        string srcPath = srcFile;
        string dstFolder = Path.Combine(_fileService.VideoFolder, video.Folder);
        Directory.CreateDirectory(dstFolder);
        string dstFile = Path.Combine(dstFolder, newFilename);

        await _videoRepo.Create(video);
        File.Move(srcPath, dstFile);
        await _taskQueue.EnqueueTask(async token => {
            try {
                await GenerateVideoCoverAsync(dstFile, Path.Combine(dstFolder, "cover.jpg"));
            }
            catch (Exception ex) {
                Console.WriteLine($"{video.Title} : {ex.Message}");
            }

        });


    }
    public async Task DeleteVideo(string id) {
        Video video = await _videoRepo.GetById(id);
        var path = Path.Combine(_fileService.VideoFolder, video.Folder);
        if (Directory.Exists(path))
            Directory.Delete(path, true);
        await _videoRepo.Delete(id);
    }
    public async Task UpdateVideo(string id, Video updated, string oldTitle) {
        if (oldTitle == updated.Title) {
            await _videoRepo.Update(id, updated);
            return;
        }

        string oldDir = Path.Combine(_fileService.VideoFolder, $"[{updated.Code}]{oldTitle}");
        string newDir = Path.Combine(_fileService.VideoFolder, updated.Folder);

        // 1. 確保舊資料夾存在
        if (!Directory.Exists(oldDir))
            throw new DirectoryNotFoundException($"找不到舊資料夾: {oldDir}");

        // 2. 改資料夾名稱
        Directory.Move(oldDir, newDir);

        // 3. 改影片檔名稱
        string oldFile = Path.Combine(newDir, $"{oldTitle}.mp4");
        string newFile = Path.Combine(newDir, $"{updated.Title}.mp4");

        if (File.Exists(oldFile)) {
            File.Move(oldFile, newFile);
        }

        await _videoRepo.Update(id, updated);
    }

    private async Task GenerateVideoCoverAsync(string videoPath, string coverPath, int second = 3) {

        var psi = new ProcessStartInfo {
            FileName = "ffmpeg.exe",
            Arguments = $"-ss {second} -i \"{videoPath}\" -frames:v 1 -q:v 2 \"{coverPath}\" -y",
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };
        using var proc = Process.Start(psi);
        if (proc == null) throw new Exception("Can't launch FFMPEG");
        string stderr = await proc.StandardError.ReadToEndAsync();
        await proc.WaitForExitAsync();

        if (proc.ExitCode != 0) {
            throw new Exception($"FFMPEG failed to generate cover.");
        }
    }
}
