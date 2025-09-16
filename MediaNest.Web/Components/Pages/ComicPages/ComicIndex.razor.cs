using MediaNest.Shared.Entities;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace MediaNest.Web.Components.Pages.ComicPages;

public partial class ComicIndex : ComponentBase {
    [Inject] public ApiClient ApiClient { get; set; }
    [Inject] public AuthenticationStateProvider AuthProvider { get; set; }  // TODO : Authorization
    [Parameter] public string SearchText { get; set; }

    private List<Comic> _comics;
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
                    _comics = [];
                    return;
                }
                await GetComicsByPage(1);
            }
        }
    }
    private async Task GetTotalPage() {
        int total = await ApiClient.GetAsync<int>("/api/comic/count");
        _totalPage = (int)Math.Ceiling(total / (double)_elementPerPage);
    }

    private async Task SetPage(int page) {
        await GetComicsByPage(page);
    }

    private async Task LastPage() {
        _currentPage--;
        await GetComicsByPage(_currentPage);

    }
    private async Task NextPage() {
        _currentPage++;
        await GetComicsByPage(_currentPage);
    }
    private async Task GetComicsByPage(int page) {
        if (_totalPage <= 0) {
            _comics = [];
            return;
        }

        page = Math.Clamp(page, 1, _totalPage);
        _currentPage = page;

        _comics = await ApiClient.GetAsync<List<Comic>>($"/api/comic?page={page}&count={_elementPerPage}");
    }
    private async Task GetRandomComics() {

        _comics = await ApiClient.GetAsync<List<Comic>>($"api/comic?random=true&count={_elementPerPage}");
    }
    private async Task HandleSearch() {

        if (string.IsNullOrWhiteSpace(SearchText)) {
            await GetComicsByPage(1);
        }
        else {
            _comics = await ApiClient.GetAsync<List<Comic>>($"api/comic?term={SearchText}");
        }
    }

}
