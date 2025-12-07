using MediaNest.Shared.Entities.Interfaces;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MediaNest.Shared.Entities;

public class Music : IEntity {
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

    public List<string> Performers { get; set; } = [];
    public List<string> Tags { get; set; } = [];
    public string Code { get; set; } = string.Empty;

    [BsonIgnore]
    public string Filename => $"[{Code}]{Title}";
    public static Dictionary<string, SearchFieldType> SearchableFields => new() {
        {"Title" , SearchFieldType.Regex },
        {"Performers", SearchFieldType.Contains },
        {"Tags", SearchFieldType.Contains },
        {"Code", SearchFieldType.Equals }
    };


}
