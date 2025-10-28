using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace MediaNest.Shared.Entities;

public class Anime : IEntity {

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
    public static Dictionary<string, SearchFieldType> SearchableFields => new() {
        { "Title",       SearchFieldType.Regex },
        { "Code",        SearchFieldType.Equals },
        { "Tags",        SearchFieldType.Contains }
    };
}
