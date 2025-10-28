﻿using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace MediaNest.Shared.Entities;

public class MusicList : IEntity {
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    [Required]
    public string Title { get; set; } = string.Empty;
    public List<string> MusicIds { get; set; } = [];
    public List<string> Tags { get; set; } = [];
    public string Description { get; set; } = string.Empty;

    public string Code { get; set; } = string.Empty;
    public static Dictionary<string, SearchFieldType> SearchableFields => new() {
        {"Title", SearchFieldType.Regex },
        {"Tags" ,  SearchFieldType.Contains},
        {"Description" , SearchFieldType.Regex }
    };

    
}
