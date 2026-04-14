using MediaNest.Shared.Entities.Interfaces;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
namespace MediaNest.Shared.Entities;

public class Figure : IRepoEntity {
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }
    public string Name { get; set; }
    public string Series { get; set; }
    public string Code { get; set; }
    [BsonIgnore]  // as index view card cover, full url : $AssetFolder/Figures/_Purikura/[Code]Name
    public string PurikuraUrl => $"_Purikura/[{Code}][{Name}]";
    [BsonIgnore]  // for detail view show all image link to the figure, full url : $AssetFolder/Figures/Series/Name
    public string Folder => $"{Series}/{Name}";
    public static IReadOnlyDictionary<string, SearchFieldType> SearchableFields { get; set; } = new Dictionary<string, SearchFieldType>() {
        {nameof(Name), SearchFieldType.Contains},
        {nameof(Code), SearchFieldType.Equals},
    };
}


