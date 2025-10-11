using MediaNest.Shared.Entities;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace MediaNest.Web.Components.Pages.ComicPages.ComicListRelated;

public partial class ComicListIndex : ComponentBase {
    [Inject] public ApiClient ApiClient { get; set; }
    [Inject] public AuthenticationStateProvider AuthProvider { get; set; }  // TODO : Authorization
    [Parameter] public string SearchText { get; set; }

    private List<ComicList> _comicLists = [];
    private List<Comic> _coverComics = [];
    private int _elementPerPage = 30;
    private int _currentPage;
    private int _totalPage;

    protected override async Task OnInitializedAsync() {

        var authState = await AuthProvider.GetAuthenticationStateAsync();
        if (authState.User.Identity?.IsAuthenticated ?? false) {
            if (!string.IsNullOrEmpty(SearchText)) {
                await HandleSearch();
            }
            else {
                await GetTotalPage();
                if (_totalPage <= 0) {
                    _comicLists = [];
                    return;
                }
                await GetComicListsByPage(1);
            }
            await SetupCoverComics();

        }
    }
    private async Task SetupCoverComics() {
        _coverComics.Clear();
        foreach (var comicList in _comicLists) {
            var comic = await ApiClient.GetAsync<Comic>($"/api/comic/{comicList.ComicIds[0]}");
            _coverComics.Add(comic);
        }
    }
    private async Task GetTotalPage() {
        int total = await ApiClient.GetAsync<int>("/api/comiclist/count");
        _totalPage = (int)Math.Ceiling(total / (double)_elementPerPage);
    }

    private async Task SetPage(int page) {
        await GetComicListsByPage(page);
    }

    private async Task LastPage() {
        _currentPage--;
        await GetComicListsByPage(_currentPage);

    }
    private async Task NextPage() {
        _currentPage++;
        await GetComicListsByPage(_currentPage);
    }
    private async Task GetComicListsByPage(int page) {
        if (_totalPage <= 0) {
            _comicLists = [];
            return;
        }

        page = Math.Clamp(page, 1, _totalPage);
        _currentPage = page;

        _comicLists = await ApiClient.GetAsync<List<ComicList>>($"/api/comiclist?page={page}&count={_elementPerPage}");
    }
    private async Task GetRandomComics() {

        _comicLists = await ApiClient.GetAsync<List<ComicList>>($"/api/comiclist?random=true&count={_elementPerPage}");
    }
    private async Task HandleSearch() {

        if (string.IsNullOrWhiteSpace(SearchText)) {
            await GetComicListsByPage(1);
        }
        else {
            _comicLists = await ApiClient.GetAsync<List<ComicList>>($"/api/comiclist?term={SearchText}");
        }
    }

}
