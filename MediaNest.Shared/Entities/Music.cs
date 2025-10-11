using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace MediaNest.Shared.Entities;

public class Music {
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;
    [Required]
    [RegularExpression(@"^[^\/:*?""<>|]+$", ErrorMessage = "Title 不可包含 / \\ : * ? \" < > | 這些字元")]
    public string Title { get; set; } = string.Empty;
    public List<string> Tags { get; set; } = [];
    public string Uploader { get; set; } = string.Empty;
    public string Code { get; set; } = "xxxxxx";

    [BsonIgnore]
    public string FolderName => Code[..3];

}
