using MediaNest.Shared.Entities.Interfaces;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MediaNest.Shared.Entities;

public class Music : IRepoEntity {
    public Music() {
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
    public List<string> Performers { get; set; } = [];
    public List<string> Tags { get; set; } = [];

    [BsonIgnore]
    public string Filename => $"[{Code}]{Title}";
    public static IReadOnlyDictionary<string, SearchFieldType> SearchableFields { get; set; } = new Dictionary<string, SearchFieldType>() {
        {nameof(Title) , SearchFieldType.Like },
        {nameof(Performers), SearchFieldType.Contains },
        {nameof(Tags), SearchFieldType.Contains },
        {nameof(Code), SearchFieldType.Equals }
    };


}
