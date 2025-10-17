using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace MediaNest.Shared.Entities;

public class ComicList {

    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    [Required]
    public string Title { get; set; } = string.Empty;
    public List<string> ComicIds { get; set; } = [];
    public List<string> Tags { get; set; } = [];
    public string Description { get; set; } = string.Empty;

    [BsonIgnore]
    public int Episodes => ComicIds.Count;
}
