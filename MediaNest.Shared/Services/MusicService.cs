using MediaNest.Shared.Entities;

namespace MediaNest.Shared.Services;

public class MusicService(EntityRepository<Music> _musicRepo, EntityRepository<MusicList> _listRepo, FileService _fileService) {
    public async Task<List<Music>> GetAllMusic() => await _musicRepo.GetAll();
    public async Task<List<Music>> Search(string term) => await _musicRepo.Search(term);
    public async Task<List<Music>> GetPage(int page, int count) => await _musicRepo.GetByPage(page, count);
    public async Task CreateMusic(string srcFile) {
        Music music = new() {
            Title = Path.GetFileNameWithoutExtension(srcFile)
        };
        string parentDir = Path.GetDirectoryName(srcFile);
        string oldFilename = Path.GetFileName(srcFile);
        string newFilename = $"{music.Filename}.mp3";
        string srcPath = Path.Combine(_fileService.TaskFolder, oldFilename);
        string dstPath = Path.Combine(_fileService.MusicFolder, newFilename);
        File.Move(srcPath, dstPath);
        await _musicRepo.Create(music);
    }
    public async Task DeleteMusic(string id) {
        Music music = await _musicRepo.GetById(id);
        var path = Path.Combine(_fileService.MusicFolder, music.Filename);
        File.Delete(path);
        await _musicRepo.Delete(id);
    }
    public async Task UpdateMusic(string id, Music updated, string oldTitle) {
        await _musicRepo.Update(id, updated);
        if (oldTitle == updated.Title) return;
        var srcPath = Path.Combine(_fileService.MusicFolder, oldTitle);
        var dstPath = Path.Combine(_fileService.MusicFolder, updated.Filename);
        File.Move(srcPath, dstPath);
    }

}
