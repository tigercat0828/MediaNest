using MediaNest.Shared.Entities.Interfaces;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace MediaNest.Shared.Entities;

public class VideoList : IRepoEntity {
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;
    [Required]
    public string Title { get; set; } = string.Empty;
    public string Code { get; set; }
    public List<string> Tags { get; set; } = [];
    public string Description { get; set; } = string.Empty;
    public List<string> VideoIds { get; set; } = [];
    public static IReadOnlyDictionary<string, SearchFieldType> SearchableFields { get; set; } = new Dictionary<string, SearchFieldType>() {
        {nameof(Title), SearchFieldType.Like },
        {nameof(Tags) ,  SearchFieldType.Contains},
        {nameof(Description) , SearchFieldType.Like }
    };
}
