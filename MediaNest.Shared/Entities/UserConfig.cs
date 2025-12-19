using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MediaNest.Shared.Entities;

public class UserConfig {
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }
    public string Username { get; set; }
    public string HomeTitle { get; set; }
    public List<string> BackgroundImageUrls { get; set; }
    public List<string> CommonTags { get; set; }
    public List<EntityCollection> MenuPinnedCollections { get; set; }
    public List<EntityCollection> HomePinnedCollections { get; set; }
}
public class EntityCollection {
    public string Title { get; set; }
    [BsonRepresentation(BsonType.String)]
    public CollectionType Type { get; set; }
    public List<string> IncludedTags { get; set; }
}
public enum CollectionType {
    Music, Video, Comic, Image, Figure
}
