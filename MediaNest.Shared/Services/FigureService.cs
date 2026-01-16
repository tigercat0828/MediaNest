using MediaNest.Shared.Entities;
using Microsoft.AspNetCore.Components.Authorization;

namespace MediaNest.Shared.Services;

public class FigureService(
    EntityRepository<Figure> _figures,
    FileService _fileService,
    AuthenticationStateProvider authStateProvider
    ) : BaseService(authStateProvider) {
    public async Task<Figure> GetFigure(string id) {
        return await _figures.GetById(id);
    }

    public async Task<List<Figure>> GetFiguresByPage(int page, int count) {
        return await _figures.GetByPage(page, count);
    }

    public async Task<List<Figure>> SearchFigures(string term) {
        return await _figures.Search(term);
    }
    public async Task CreateFigure(Figure figure) {
        if (!await AuthorizeAsync(UserRole.User)) return;
        await _figures.Create(figure);
    }
    public async Task UpdateFigure(string id, Figure updated) {
        if (!await AuthorizeAsync(UserRole.User)) return;
        await _figures.Update(id, updated);
    }
    public async Task DeleteFigure(string id) {
        if (!await AuthorizeAsync(UserRole.User)) return;
        var figure = await _figures.GetById(id);
        if (figure != null) {
            await _figures.Delete(id);
            string filePath = Path.Combine(_fileService.FigureFolder, figure.Title, figure.Name);
            if (File.Exists(filePath)) {
                File.Delete(filePath);
            }
        }
    }
    public async Task<List<Figure>> GetRandomFigures(int count) {
        return await _figures.GetRandom(count);
    }
}