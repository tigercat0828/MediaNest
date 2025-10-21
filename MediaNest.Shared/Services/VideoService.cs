using MediaNest.Shared.Entities;

namespace MediaNest.Shared.Services;

public class VideoService(EntityRepository<Video> _videoRepo, EntityRepository<VideoList> _listRepo, FileService _fileService) {


}
