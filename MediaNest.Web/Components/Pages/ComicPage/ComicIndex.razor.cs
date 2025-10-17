using MediaNest.Shared.Dtos;
using MediaNest.Shared.Entities;
using MediaNest.Shared.Services;
using MediaNest.Web.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;

namespace MediaNest.Web.Components.Pages.ComicPage;

public partial class ComicIndex : ComponentBase {
    [CascadingParameter] private Task<AuthenticationState> _authStateTask { get; set; }
    [Parameter] public string SearchText { get; set; } = null!;
    [Inject] public ComicService ComicService { get; set; } = null!;
    [Inject] public ComicCartService ComicCartService { get; set; }
    private List<Comic> _comics = [];
    private int _elementPerPage = 30;
    private int _currentPage;
    private int _totalPage;
    private bool _isSelectMode = false;
    private async Task SetPage(int page) {
        _currentPage = page;
        await GetComicsByPage(page);
    }

    protected override async Task OnInitializedAsync() {
        var authState = await _authStateTask;
        if (authState.User.Identity?.IsAuthenticated ?? false) {
            if (!string.IsNullOrEmpty(SearchText)) {
                await HandleSearch();
            }
            else {
                await GetTotalPage();
                if (_totalPage <= 0) {
                    _comics = [];
                    return;
                }
                await GetComicsByPage(1);
            }
        }
    }
    private async Task GetTotalPage() {
        int total = await ComicService.GetCount();
        _totalPage = (int)Math.Ceiling(total / (double)_elementPerPage);
    }
    private async Task GetComicsByPage(int page) {
        if (_totalPage <= 0) {
            _comics = [];
            return;
        }

        page = Math.Clamp(page, 1, _totalPage);
        _currentPage = page;

        _comics = await ComicService.GetComics(page, _elementPerPage);
    }
    private async Task GetRandomComics() {
        _comics = await ComicService.GetRandomComics(_elementPerPage);
    }
    private async Task HandleSearch() {

        if (string.IsNullOrWhiteSpace(SearchText))
            await GetComicsByPage(1);
        else
            _comics = await ComicService.SearchComic(SearchText);
    }
    private async Task OnKeyDown(KeyboardEventArgs e) {
        if (e.Key == "Enter") {
            await HandleSearch();
        }
    }
    private async Task AddToCart(Comic comic) {
        await ComicCartService.AddAsync(
            new ComicDto {
                Id = comic.Id,
                Title = comic.Title,
                Folder = comic.Folder,
            });
    }
    private void ToggleSelectMode() {
        _isSelectMode = !_isSelectMode;
    }
}
