using MediaNest.ApiService.Services;
using MediaNest.Shared.Entities;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;


namespace MediaNest.ApiService.Endpoints; 
public static class ComicServiceEndpoints {
    public const int DEFAULT_COUNT = 5;
    public static void MapComicServiceEndpoints(this IEndpointRouteBuilder builder) {
        var group = builder
            .MapGroup("/api/comic")
            .RequireAuthorization(new AuthorizeAttribute { Roles = "Admin,User" })
            .WithTags("Comic");

        // [HttpGet]
        group.MapGet("/", GetComics);   // random, search, paging
        group.MapGet("/count", GetCount);
        group.MapGet("/{id}", GetComicById);
        // [HttpPost]
        group.MapPost("/", CreateComic);
        // [HttpPut]
        group.MapPut("/{id}", UpdateComic);
        // [HttpDelete]
        group.MapDelete("/{id}", DeleteComic);
    }
    private static async Task<IResult> GetComicById(ComicService service, string id) {
        var comic = await service.GetComicById(id);
        if (comic == null) 
            return Results.NotFound();
        else 
            return Results.Ok(comic);
    }
    private static async Task<IResult> GetComics(ComicService service, string? search, int? page, int? count, bool? random) {
        List<Comic> comics;
        if (!string.IsNullOrEmpty(search)) {
            comics = await service.SearchComic(search);
        }
        else if (random == true) {
            int cnt = count.HasValue ? Math.Min(count.Value, DEFAULT_COUNT) : DEFAULT_COUNT; 
            comics = await service.GetRandomComics(cnt);
        }
        else if (page.HasValue) {
            int cnt = count.HasValue ? Math.Min(count.Value, DEFAULT_COUNT) : DEFAULT_COUNT;
            comics = await service.GetComics(page.Value, cnt);
        }
        else {
            comics = [];
        }
        return Results.Ok(comics);
    }
    private static async Task<IResult> GetCount(ComicService service) {
        return Results.Ok(await service.GetCount());
    }
    private static async Task<IResult> CreateComic(ComicService service, ClaimsPrincipal user, Comic comic) {
        
        comic.Uploader = user.Identity?.Name ?? "Unknown";
        await service.CreateComic(comic);
        return Results.Created($"/api/comic/{comic.Id}", comic); // 回傳 Comic 本身
    }
    private static async Task<IResult> DeleteComic(ComicService service, string id) {
        bool exist = await service.CheckExists(id);
        if (exist) { 
            await service.DeleteComic(id);  // TODO : delete the folder and its content
            return Results.Ok("success");
        }
        else 
            return Results.NotFound();
    }
    private static async Task<IResult> UpdateComic(ComicService service, string id, Comic updatedComic) {
        bool exists = await service.CheckExists(id);
        if (exists) { 
            await service.UpdateComic(id, updatedComic);    // TODO : change folder name
            return Results.Ok(updatedComic);
        }
        else {
            return Results.NotFound();
        }
    }
}
