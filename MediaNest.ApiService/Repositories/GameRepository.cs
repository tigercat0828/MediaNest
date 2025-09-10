using MediaNest.Shared.Entities;
using MediaNest.Web.Database;
using Microsoft.EntityFrameworkCore;

namespace MediaNest.Web.Repositories;

public interface IGameRepository {
    Task<List<Game>> GetAllGames();
    Task<Game> GetGameById(int id);
    Task<Game> CreateGame(Game game);
    Task<bool> CheckExist(int id);
    Task UpdateGame(Game game);
    Task DeleteGame(int id);
}
public class GameRepository(AppDbContext dbContext) : IGameRepository {
    public Task<List<Game>> GetAllGames() {
        return dbContext.Games.ToListAsync();
    }
    public async Task<Game> CreateGame(Game game) {
        dbContext.Games.Add(game);
        await dbContext.SaveChangesAsync();
        return game;
    }
    public async Task<Game> GetGameById(int id) {
        return await dbContext.Games.FirstOrDefaultAsync(g => g.Id == id);
    }

    public Task UpdateGame(Game game) {
        dbContext.Entry(game).State = EntityState.Modified;
        return dbContext.SaveChangesAsync();
    }
    public Task<bool> CheckExist(int id) {
        return dbContext.Games.AnyAsync(g => g.Id == id);
    }

    public async Task DeleteGame(int id) {
        var game = dbContext.Games.FirstOrDefaultAsync(g => g.Id == id);
        dbContext.Games.Remove(game.Result);
        await dbContext.SaveChangesAsync();
    }
}
