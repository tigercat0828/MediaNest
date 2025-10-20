using MediaNest.Shared.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using ZstdSharp.Unsafe;

namespace MediaNest.Shared.Services;

using ComicRepository = EntityRepository<Comic, ComicList>;
public class ComicService(ComicRepository _comicRepo, FileService _fileService) {
    // entitiy
    public async Task<List<Comic>> SearchComic(string term) => await _comicRepo.Search(term);
    public async Task<int> GetCount() => await _comicRepo.GetCount();
    public async Task<Comic> GetById(string id) => await _comicRepo.GetById(id);
    public async Task<List<Comic>> GetComics(int page, int count) => await _comicRepo.GetEntities(page, count);
    public async Task<List<Comic>> GetRandomComics(int count) => await _comicRepo.GetRandomEntities(count);
    public async Task UpdateComicWithoutFileOperation(string id, Comic updated) => await _comicRepo.Update(id, updated);
    // list
    public async Task<List<ComicList>> SearchList(string term) => await _comicRepo.SearchList(term);
    public async Task<List<ComicList>> GetLists(int page, int count) =>  await _comicRepo.GetLists(page, count); 
    public async Task<int> GetListCount() => await _comicRepo.GetListCount();
    public async Task<ComicList> GetListById(string id) => await _comicRepo.GetListById(id);
    public async Task<List<ComicList>> GetRandomLists(int count) => await _comicRepo.GetRandomLists(count);
    public async Task DeleteComicList(string id) => await _comicRepo.DeleteList(id);
}