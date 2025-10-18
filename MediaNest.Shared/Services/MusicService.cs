using MediaNest.Shared.Entities;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace MediaNest.Shared.Services; 
public class MusicService (IMongoCollection<Music> _musics, IMongoCollection<MusicList> _lists){
    // entity
    public async Task<List<Music>> GetAllMusic() {
        return await  _musics.Find(_ => true).ToListAsync();
    }
    public async Task<Music> GetMusicById(string id) { 
        return await _musics.Find(m => m.Id == id).FirstOrDefaultAsync();
    }
    public async Task CreateMusic(Music music) { 
        await _musics.InsertOneAsync(music);
    }
    public async Task UpdateMusic(string id, Music updatedMusic) { 
        await _musics.ReplaceOneAsync(x =>x.Id == id, updatedMusic);
    }
    public async Task DeleteMusic(string id) { 
        await _musics.DeleteOneAsync(x => x.Id == id);
    }
    // list
    public async Task<List<MusicList>> GetAllMusicList() {
        return await _lists.Find(_ => true).ToListAsync();
    }
    public async Task<MusicList> GetMusicListById(string id) {
        return await _lists.Find(m => m.Id == id).FirstOrDefaultAsync();
    }
    public async Task CreateMusicList(MusicList music) {
        await _lists.InsertOneAsync(music);
    }
    public async Task UpdateMusicList(string id, MusicList list) {
        await _lists.ReplaceOneAsync(x => x.Id == id, list);
    }
    public async Task DeleteMusicList(string id) {
        await _lists.DeleteOneAsync(x => x.Id == id);
    }
}
