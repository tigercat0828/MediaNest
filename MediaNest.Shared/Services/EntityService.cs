using MediaNest.Shared.Entities;
using MongoDB.Driver;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace MediaNest.Shared.Services; 
public class EntityService<T, TList>(IMongoCollection<T> _entities, IMongoCollection<TList> _lists) 
    where T: IEntity 
    where TList : IEntity{

    public async Task<List<T>> GetAll() {
        return await _entities.Find(_ => true).ToListAsync();
    }
    public async Task<T> GetById(string id) {
        return await _entities.Find(m => m.Id == id).FirstOrDefaultAsync();
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
