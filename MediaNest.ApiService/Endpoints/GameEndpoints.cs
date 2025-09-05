using MediaNest.ApiService.Services;
using MediaNest.Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Identity.Client;
using System.Runtime.CompilerServices;

namespace MediaNest.ApiService.Endpoints; 
public static class GameEndpoints {
    public static void MapGameEndpoints(this IEndpointRouteBuilder routes) {

        var group = routes.MapGroup("/api/game").WithTags("Game");
        
        // [HttpGet]
        group.MapGet("/", GetAllGames);
        group.MapGet("/{id:int}", GetGameById);
        // [HttpPost]
        group.MapPost("/", CreateGame);
        // [HttpPut]
        group.MapPut("/{id:int}", UpdateGame);
        // [HttpDelete]
        group.MapDelete("/{id:int}", DeleteGame);
    }

    public static async Task<IResult> GetAllGames(IGameService gameService) {
        List<Game> products = await gameService.GetAllGames();

        if (products.Count == 0) 
            return TypedResults.NoContent();
        else if (products == null) 
            return TypedResults.NotFound();
        
        return TypedResults.Ok(products);
    }
    public static async Task<IResult> CreateGame(IGameService gameService, Game newGame) {
        Game game = await gameService.CreateGame(newGame);
        return TypedResults.Ok(new BaseResponse<Game>(true, newGame));
    }
    public static async Task<IResult> GetGameById(IGameService gameService, int id) {
        Game game = await gameService.GetGameById(id);
        if (game == null) 
            return TypedResults.NotFound(new BaseResponse<Game>(false,"Game not found", null));
        return TypedResults.Ok(new BaseResponse<Game>(true, game));
    }
    public static async Task<IResult> UpdateGame(IGameService gameService, int id, Game updatedGame) {
        if(id != updatedGame.Id || !await gameService.CheckExist(id)) {
            return TypedResults.BadRequest(new BaseResponse<Game>(false, "Game ID mismatch or Game not found", null));
        }
        await gameService.UpdateGame(updatedGame);
        return TypedResults.Ok(new BaseResponse<Game> (true, updatedGame));
    }
    public static async Task<IResult> DeleteGame(IGameService gameService, int id) {
        if (!await gameService.CheckExist(id)) {
            return TypedResults.NotFound(new BaseResponse<Game>(false, "Game not found", null));
        }
        await gameService.DeleteGame(id);
        return TypedResults.Ok(new BaseResponse<Game>(true, "Game deleted successfully", null));
    }
}
