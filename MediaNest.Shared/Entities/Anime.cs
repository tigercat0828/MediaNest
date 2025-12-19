using MediaNest.Shared.Entities.Interfaces;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MediaNest.Shared.Entities;

public class Anime : IRepoEntity {

    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public string Title {
        get => _title;
        set { _title = Utility.SanitizeTitle(value); }
    }
    private string _title = string.Empty;
    public string Code { get; set; } = Utility.GenerateSixDigitCode();
    public List<string> Tags { get; set; } = [];
    public static IReadOnlyDictionary<string, SearchFieldType> SearchableFields { get; set; } = new Dictionary<string, SearchFieldType>() {
        { nameof(Title),       SearchFieldType.Like },
        { nameof(Code),        SearchFieldType.Equals },
        { nameof(Tags),        SearchFieldType.Contains }
    };
}
