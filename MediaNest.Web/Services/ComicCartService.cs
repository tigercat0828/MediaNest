using MediaNest.Shared.Dtos;
using MediaNest.Shared.Entities;
using MediaNest.Shared.Services;
using Microsoft.JSInterop;
using System.Text.Json;

namespace MediaNest.Web.Services {
    public class ComicCartService(ComicService _comicService, IJSRuntime JS) {
        private const string StorageKey_Comics = "MediaNest.CartComics";
        private const string StorageKey_ComicList = "MediaNest.CartComicList";

        public ComicList SelectedComicList { get; set; } = new();
        public List<ComicDto> SelectedComics { get; set; } = [];

        public event Action? OnChange;

        public async Task SetComicList(ComicList list) {
            SelectedComicList = list;
        }

        public async Task CreateComicList(string Title) {
            var comicList = new ComicList();
            foreach (var comic in SelectedComics) {
                comicList.ComicIds.Add(comic.Id);
            }
            await _comicService.CreateList(comicList);
        }
        public async Task UpdateComicList() {
            var id = SelectedComicList.Id;
            var ids = SelectedComics.Select(x => x.Id);
            SelectedComicList.ComicIds.AddRange(ids);
            await _comicService.UpdateList(id, SelectedComicList);
        }

        public async Task InitializeAsync() {
            try {
                var comicsJson = await JS.InvokeAsync<string>("localStorage.getItem", StorageKey_Comics);
                if (!string.IsNullOrEmpty(comicsJson)) {
                    var comics = JsonSerializer.Deserialize<List<ComicDto>>(comicsJson);
                    if (comics != null) SelectedComics = comics;
                }
                var listJson = await JS.InvokeAsync<string>("localStorage.getItem", StorageKey_ComicList);
                if (!string.IsNullOrEmpty(listJson)) {
                    var list = JsonSerializer.Deserialize<ComicList>(listJson);
                    if (list != null) SelectedComicList = list;
                }
            }
            catch (Exception ex) {
                // ignore
            }
            OnChange?.Invoke();
        }
        public async Task AddAsync(ComicDto comic) {
            if (!SelectedComics.Any(c => c.Id == comic.Id)) {
                SelectedComics.Add(comic);
                await SaveAsync();
            }
        }

        public async Task RemoveAsync(ComicDto dto) {
            SelectedComics.RemoveAll(c => c.Id == dto.Id);
            await SaveAsync();
        }

        public async Task ClearAsync() {
            SelectedComics.Clear();
            await JS.InvokeVoidAsync("localStorage.removeItem", StorageKey_Comics);
            OnChange?.Invoke();
        }

        private async Task SaveAsync() {
            var json = JsonSerializer.Serialize(SelectedComics);
            await JS.InvokeVoidAsync("localStorage.setItem", StorageKey_Comics, json);
            OnChange?.Invoke();
        }
    }
}
