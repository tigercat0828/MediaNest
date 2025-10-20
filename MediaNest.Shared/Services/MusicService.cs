using MediaNest.Shared.Entities;

namespace MediaNest.Shared.Services;

using MusicRepository = EntityRepository<Music, MusicList>;
public class MusicService (MusicRepository _repository, FileService _service){
    
}
