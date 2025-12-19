using MediaNest.Shared.Entities.Interfaces;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
namespace MediaNest.Shared.Entities;

public class Image : IRepoEntity {

    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }
    public string Title { get; set; }
    public string Code { get; set; }
    public List<string> Tags { get; set; } = [];
    public static IReadOnlyDictionary<string, SearchFieldType> SearchableFields { get; set; } = new Dictionary<string, SearchFieldType>() {
        {nameof(Title) , SearchFieldType.Like },
        {nameof(Code), SearchFieldType.Equals },
        {nameof(Tags), SearchFieldType.Contains }
    };
}