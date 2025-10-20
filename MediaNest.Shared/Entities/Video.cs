using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MediaNest.Shared.Entities; 
public class Video : IEntity{
    public Video() {
        Code = Utility.GenerateSixDigitCode();
    }


    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Author { get; set; }
    public string Series { get; set; }
    public List<string> Figures { get; set; }
    public List<string> Tags { get; set; }
    
    [BsonIgnore]
    public string Folder => Code[..3];

    public static Dictionary<string, SearchFieldType> SearchableFields => new() {
        {"Title", SearchFieldType.Regex },
        {"Code" ,  SearchFieldType.Equals},
        {"Author" , SearchFieldType.Regex },
        {"Series", SearchFieldType.Regex  },
        {"Figures", SearchFieldType.Contains },
        {"Tag", SearchFieldType.Contains },
    };


}
