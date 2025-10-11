using MediaNest.ApiService.Services;
using MediaNest.Shared.Entities;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace MediaNest.ApiService.Endpoints;

public static class ComicListServiceEndpoints {
    public const int DEFAULT_COUNT = 30;
    public static void MapComicListServiceEndpoints(this IEndpointRouteBuilder builder) {
        var group = builder
            .MapGroup("/api/comiclist")
            .RequireAuthorization(new AuthorizeAttribute { Roles = "Admin,User" })
            .WithTags("ComicList");

        // [HttpGet]
        group.MapGet("/", GetComicLists);   // random, paging, search
        group.MapGet("/{id}", GetComicListById);
        group.MapGet("/count", GetCount);
        // [HttpPost]
        group.MapPost("/", CreateComicList);
        // [HttpPut]
        group.MapPut("/{id}", UpdateComicList);
        // [HttpDelete]
        group.MapDelete("/{id}", DeleteComicList);
    }
    public static async Task<IResult> GetComicListById(ComicListService service, string id) {
        var list = await service.GetComicListById(id);
        if (list == null)
            return Results.NotFound();
        else
            return Results.Ok(list);
    }
    public static async Task<IResult> GetComicLists(ComicListService service, string? term, int? page, int? count, bool? random) {
        List<ComicList> lists;
        if (!string.IsNullOrEmpty(term)) {
            lists = await service.SearchComicList(term);
        }
        else if (random == true) {
            int cnt = count.HasValue ? Math.Min(count.Value, DEFAULT_COUNT) : DEFAULT_COUNT;
            lists = await service.GetRamdomComicLists(cnt);
        }
        else if (page.HasValue) {
            int cnt = count.HasValue ? Math.Min(count.Value, DEFAULT_COUNT) : DEFAULT_COUNT;
            lists = await service.GetComicLists(page.Value, cnt);
        }
        else {
            lists = [];
        }
        return Results.Ok(lists);
    }
    public static async Task<IResult> GetCount(ComicListService service) {
        return Results.Ok(await service.GetCount());
    }
    public static async Task<IResult> CreateComicList(ComicListService service, ClaimsPrincipal user, ComicList list) {

        await service.CreateComicList(list);
        return Results.Ok();
    }
    public static async Task<IResult> UpdateComicList(ComicListService service, string id, ComicList list) {
        var existing = await service.GetComicListById(id);
        if (existing == null) {
            return Results.NotFound();
        }
        await service.UpdateComicList(id, list);
        return Results.Ok();
    }
    public static async Task<IResult> DeleteComicList(ComicListService service, string id) {
        var existing = await service.GetComicListById(id);
        if (existing == null) {
            return Results.NotFound();
        }
        await service.DeleteComicList(id);
        return Results.Ok();
    }
}
