using MediaNest.Shared.Entities.Interfaces;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MediaNest.Shared.Entities;

public class BulletinItem : IRepoEntity {
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }
    public string Title { get; set; }
    public string Code { get; set; }
    public string Content { get; set; }
    public DateTime LastUpdate { get; set; }
    public bool Pinned { get; set; }
    public static IReadOnlyDictionary<string, SearchFieldType> SearchableFields { get; set; } = new Dictionary<string, SearchFieldType>() {
        { nameof(Title),    SearchFieldType.Like },
        { nameof(Code),     SearchFieldType.Equals },
        { nameof(Content),  SearchFieldType.Like },
    };
}
