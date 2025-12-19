namespace MediaNest.Shared.Entities.Interfaces;

public enum SearchFieldType {
    Like, Equals, Contains
}

public interface IRepoEntity {
    public string Id { get; set; }
    public string Title { get; set; }
    public string Code { get; set; }
    public static abstract IReadOnlyDictionary<string, SearchFieldType> SearchableFields { get; set; }
}
