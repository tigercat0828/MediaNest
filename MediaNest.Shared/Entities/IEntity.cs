using MongoDB.Bson.Serialization.Conventions;
using System;
using System.Collections.Generic;
using System.Text;

namespace MediaNest.Shared.Entities;

public enum SearchFieldType { 
    Regex, Equals, Contains
}

public interface IEntity {
    public string Id { get; set; }
    public string Title { get; set; }
    public static abstract Dictionary<string , SearchFieldType> SearchableFields { get; }
}
