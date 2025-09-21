using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace MediaNest.Shared.Entities;

public class Comic {
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;
    [Required]
    [RegularExpression(@"^[^\/:*?""<>|]+$", ErrorMessage = "Title 不可包含 / \\ : * ? \" < > | 這些字元")]
    public string Title { get; set; } = string.Empty;
    public string SubTitle { get; set; } = string.Empty;
    public string Series { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<string> Tags { get; set; } = [];
    public List<string> Characters { get; set; } = [];
    public List<int> Bookmarks { get; set; } = [];
    public string Uploader { get; set; } = string.Empty;
    public string Code { get; set; } = "000000";
    [BsonIgnore]
    public string FolderName => Path.Combine(Code[..3], $"[{Code}]{Title}");    // retrieve asset path (physical hierarchy directories)
}
