using MediaNest.Shared.Entities.Interfaces;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace MediaNest.Shared.Entities;


public class Comic : IRepoEntity {

    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;
    [Required]
    public string Title {
        get => _title;
        set { _title = Utility.SanitizeTitle(value); }
    }

    private string _title = string.Empty;
    public string SubTitle { get; set; } = string.Empty;
    public string Series { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public List<string> Tags { get; set; } = [];
    public List<string> Characters { get; set; } = [];
    public List<int> Bookmarks { get; set; } = [];
    public string Uploader { get; set; } = string.Empty;
    public string Code { get; set; } = Utility.GenerateSixDigitCode();

    [BsonIgnore]
    public string Folder => Path.Combine(Code[..3], $"[{Code}]{Title}");    // retrieve asset path (physical hierarchy directories)

    public static IReadOnlyDictionary<string, SearchFieldType> SearchableFields { get; set; } = new Dictionary<string, SearchFieldType>() {
        { nameof(Title),       SearchFieldType.Like },
        { nameof(SubTitle),    SearchFieldType.Like },
        { nameof(Author),      SearchFieldType.Like },
        { nameof(Series),      SearchFieldType.Like },
        { nameof(Code),        SearchFieldType.Equals },
        { nameof(Tags),        SearchFieldType.Contains },
        { nameof(Characters),  SearchFieldType.Contains }
    };
}
