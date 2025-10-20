using MediaNest.Shared.Entities;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace MediaNest.Shared.Services; 
public class EntityRepository<T, TList>(IMongoCollection<T> entities, IMongoCollection<TList> lists) where T: IEntity where TList : IEntity{
    // entity
    public async Task<int> GetCount() {
        return (int)await entities.CountDocumentsAsync(_ => true);
    }
    public async Task<List<T>> GetAll() {
        return await entities.Find(_ => true).ToListAsync();
    }
    public async Task<List<T>> GetEntities(int page, int count) {
        return await entities
                .Find(_ => true)
                .SortByDescending(c => c.Id)
                .Skip((page - 1) * count)
                .Limit(count)
                .ToListAsync();
    }
    public async Task<List<T>> GetRandomEntities(int count) { 
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
    // list
    public async Task<int> GetListCount() {
        return (int)await lists.CountDocumentsAsync(_ => true);
    }
    public async Task<List<TList>> GetAllList() {
        return await lists.Find(_ => true).ToListAsync();
    }
    public async Task<List<TList>> GetLists(int page, int count) {
        return await lists
            .Find(_ => true)
            .SortByDescending(c => c.Id)
            .Skip((page - 1) * count)
            .Limit(count)
            .ToListAsync();
    }
    public async Task<List<TList>> GetRandomLists(int count) {
        return await lists.Aggregate().Sample(count).ToListAsync();
    }
    public async Task<TList> GetListById(string id) {
        return await lists.Find(m => m.Id == id).FirstOrDefaultAsync();
    }
    public async Task<List<TList>> SearchList(string term) {
        var escapedTerm = Regex.Escape(term);
        // 模糊搜尋（加上 .* 讓字串前後可以有其他文字）
        var pattern = $".*{escapedTerm}.*";
        var filters = new List<FilterDefinition<TList>>();

        foreach (var (field, type) in T.SearchableFields) {
            FilterDefinition<TList>? filter = type switch {
                SearchFieldType.Regex => Builders<TList>.Filter.Regex(field, new BsonRegularExpression(pattern, "i")),
                SearchFieldType.Equals => Builders<TList>.Filter.Eq(field, term),
                SearchFieldType.Contains => Builders<TList>.Filter.AnyIn(field, [term]),
                _ => null
            };
            if (filter is not null) filters.Add(filter);
        }
        var combined = Builders<TList>.Filter.Or(filters);
        return await lists.Find(combined).ToListAsync();
    }
    public async Task CreateList(TList music) {
        await lists.InsertOneAsync(music);
    }
    public async Task UpdateList(string id, TList list) {
        await lists.ReplaceOneAsync(x => x.Id == id, list);
    }
    public async Task DeleteList(string id) {
        await lists.DeleteOneAsync(x => x.Id == id);
    }
}
