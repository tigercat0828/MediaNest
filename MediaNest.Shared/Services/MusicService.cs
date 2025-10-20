using MediaNest.Shared.Entities;

namespace MediaNest.Shared.Services;

public class MusicService (
    EntityRepository<Music> _musicRepo,
    EntityRepository<MusicList> _listRepo,
    FileService _service) {
    
}
