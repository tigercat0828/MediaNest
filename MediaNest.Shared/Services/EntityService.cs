using MediaNest.Shared.Entities;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace MediaNest.Shared.Services; 
public class EntityService<T, TList> where T: IEntity where TList : IEntity{

    private readonly IMongoCollection<T> _entities;
    private readonly IMongoCollection<TList> _lists;
    public EntityService(IMongoCollection<T> entities, IMongoCollection<TList> lists) {
        _entities = entities;
        _lists = lists;
    }

    public async Task<List<T>> GetAll() {
        return await _entities.Find(_ => true).ToListAsync();
    }
    public async Task<T> GetById(string id) {
        return await _entities.Find(m => m.Id == id).FirstOrDefaultAsync();
    }
    public async Task<List<T>> Search(string term) {
        var escapedTerm = Regex.Escape(term);
        // 模糊搜尋（加上 .* 讓字串前後可以有其他文字）
        var pattern = $".*{escapedTerm}.*";
        var titleFilter = Builders<T>.Filter.Regex("Title", new BsonRegularExpression(pattern, "i"));
        var tagsFilter = Builders<T>.Filter.AnyIn("Tags", [term]);
        var codeFilter = Builders<T>.Filter.Eq("Code", term);
        var combinedFilter = titleFilter | codeFilter | tagsFilter ;
        return await _entities.Find(combinedFilter).ToListAsync();
    }
    public async Task Create(T entity) {
        await _entities.InsertOneAsync(entity);
    }
    public async Task Update(string id, T updatedMusic) {
        await _entities.ReplaceOneAsync(x => x.Id == id, updatedMusic);
    }
    public async Task Delete(string id) {
        await _entities.DeleteOneAsync(x => x.Id == id);
    }
    // list
    public async Task<List<TList>> GetAllList() {
        return await _lists.Find(_ => true).ToListAsync();
    }
    public async Task<TList> GetListById(string id) {
        return await _lists.Find(m => m.Id == id).FirstOrDefaultAsync();
    }
    public async Task CreateList(TList music) {
        await _lists.InsertOneAsync(music);
    }
    public async Task UpdateList(string id, TList list) {
        await _lists.ReplaceOneAsync(x => x.Id == id, list);
    }
    public async Task DeleteMusicList(string id) {
        await _lists.DeleteOneAsync(x => x.Id == id);
    }
}
