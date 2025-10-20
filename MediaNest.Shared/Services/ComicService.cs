using MediaNest.Shared.Entities;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Text;
using ZstdSharp.Unsafe;

namespace MediaNest.Shared.Services;


public class ComicService(
    EntityRepository<Comic> _comicRepo,
    EntityRepository<ComicList> _listRepo,
    FileService _fileService) {
    // entitiy
    public async Task<List<Comic>> SearchComic(string term) => await _comicRepo.Search(term);
    public async Task<int> GetComicCount() => await _comicRepo.GetCount();
    public async Task<Comic> GetComicById(string id) => await _comicRepo.GetById(id);
    public async Task<List<Comic>> GetComics(int page, int count) => await _comicRepo.GetByPage(page, count);
    public async Task<List<Comic>> GetRandomComics(int count) => await _comicRepo.GetRandom(count);
    public async Task UpdateComicWithoutFileOperation(string id, Comic updated) => await _comicRepo.Update(id, updated);
    // list
    public async Task<List<ComicList>> SearchList(string term) => await _listRepo.Search(term);
    public async Task<List<ComicList>> GetLists(int page, int count) =>  await _listRepo.GetByPage(page, count); 
    public async Task<int> GetListCount() => await _listRepo.GetCount();
    public async Task<ComicList> GetListById(string id) => await _listRepo.GetById(id);
    public async Task<List<ComicList>> GetRandomLists(int count) => await _listRepo.GetRandom(count);
    public async Task DeleteList(string id) => await _listRepo.Delete(id);
    public async Task UpdateList(string id, ComicList updated) => await _listRepo.Update(id, updated);
    public async Task CreateList(ComicList list) => await _listRepo.Create(list);
}