using MediaNest.Shared.Entities.Interfaces;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MediaNest.Shared.Entities;

public class Video : IRepoEntity {
    public Video() {
        Code = Utility.GenerateSixDigitCode();
    }

    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;
    public string Title {
        get => _title;
        set { _title = Utility.SanitizeTitle(value); }
    }
    private string _title = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public List<string> Tags { get; set; } = [];

    [BsonIgnore]
    public string Folder => Path.Combine($"{Code[..3]}", $"[{Code}]{Title}");

    public static IReadOnlyDictionary<string, SearchFieldType> SearchableFields { get; set; } = new Dictionary<string, SearchFieldType>() {

        {nameof(Title), SearchFieldType.Like },
        {nameof(Code) ,  SearchFieldType.Equals},
        {nameof(Author) , SearchFieldType.Like },
        {nameof(Tags), SearchFieldType.Contains },
    };
}
