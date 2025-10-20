using MediaNest.Shared.Entities;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Text.RegularExpressions;

namespace MediaNest.Shared.Services;

public class EntityRepository<T>(IMongoCollection<T> entities) where T : IEntity {
    // entity
    public async Task<int> GetCount() {
        return (int)await entities.CountDocumentsAsync(_ => true);
    }
    public async Task<List<T>> GetAll() {
        return await entities.Find(_ => true).ToListAsync();
    }
    public async Task<List<T>> GetByPage(int page, int count) {
        return await entities
                .Find(_ => true)
                .SortByDescending(c => c.Id)
                .Skip((page - 1) * count)
                .Limit(count)
                .ToListAsync();
    }
    public async Task<List<T>> GetRandom(int count) {
        return await entities.Aggregate().Sample(count).ToListAsync();
    }
    public async Task<T> GetById(string id) {
        return await entities.Find(m => m.Id == id).FirstOrDefaultAsync();
    }
    public async Task<List<T>> Search(string term) {
        var escapedTerm = Regex.Escape(term);
        // 模糊搜尋（加上 .* 讓字串前後可以有其他文字）
        var pattern = $".*{escapedTerm}.*";
        var filters = new List<FilterDefinition<T>>();

        foreach (var (field, type) in T.SearchableFields) {
            FilterDefinition<T>? filter = type switch {
                SearchFieldType.Regex => Builders<T>.Filter.Regex(field, new BsonRegularExpression(pattern, "i")),
                SearchFieldType.Equals => Builders<T>.Filter.Eq(field, term),
                SearchFieldType.Contains => Builders<T>.Filter.AnyIn(field, [term]),
                _ => null
            };
            if (filter is not null) filters.Add(filter);
        }
        var combined = Builders<T>.Filter.Or(filters);
        return await entities.Find(combined).ToListAsync();
    }
    public async Task Create(T entity) {
        await entities.InsertOneAsync(entity);
    }
    public async Task Update(string id, T updated) {
        await entities.ReplaceOneAsync(x => x.Id == id, updated);
    }
    public async Task Delete(string id) {
        await entities.DeleteOneAsync(x => x.Id == id);
    }
}
