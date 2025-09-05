using MediaNest.ApiService.Repositories;
using MediaNest.Shared;

namespace MediaNest.ApiService.Services;

public interface IGameService {
    Task<List<Game>> GetAllGames();
    Task<Game> GetGameById(int id);
    Task<Game> CreateGame(Game game);
    Task UpdateGame(Game game);
    Task<bool> CheckExist(int id);
    Task DeleteGame(int id);
}

public class GameService(IGameRepository gameRepository) : IGameService {
    public Task<List<Game>> GetAllGames() {
        return gameRepository.GetAllGames();
    }
    public Task<Game> CreateGame(Game game) {
        return gameRepository.CreateGame(game);
    }

    public Task<Game> GetGameById(int id) {
        return gameRepository.GetGameById(id);
    }

    public Task UpdateGame(Game game) {
        return gameRepository.UpdateGame(game);
    }

    public Task<bool> CheckExist(int id) {
        return gameRepository.CheckExist(id);
    }

    public Task DeleteGame(int id) {
        return gameRepository.DeleteGame(id);
    }
}
