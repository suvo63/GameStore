using GameStore.Api.Authorization;
using GameStore.Api.Dtos;
using GameStore.Api.Entities;
using GameStore.Api.Repositories;

namespace GameStore.Api.Endpoints;

public static class GamesEndpoints
{
    const string GetGameV1EndpointName = "GetGameV1";
    const string GetGameV2EndpointName = "GetGameV2";

    public static RouteGroupBuilder MapGamesEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.NewVersionedApi()
                        .MapGroup("/games") // query string versioning "/games?api-version=2.0"
                                            //.MapGroup("/v{version:apiVersion}/games") //path versioning "v2/games
                        .HasApiVersion(1.0)
                        .HasApiVersion(2.0)
                        .WithParameterValidation();

        // V1 GET ENDPOINTS
        group.MapGet("/", async (
            IGamesRepository _repository,
            ILoggerFactory loggerFactory,
            HttpContext _http,
            [AsParameters] GetGamesDtoV1 request
            ) =>
        {
            var logger = loggerFactory.CreateLogger("Games Endpoints");
            logger.LogInformation("Get all endpint v1");

            var totalCount = await _repository.CountAsync();
            _http.Response.AddPaginationHeader(totalCount, request.PageSize);

            return Results.Ok((await _repository.GetAllAsync(request.PageNumber, request.PageSize))
                                                .Select(game => game.AsDtoV1()));
        })
        .MapToApiVersion(1.0);

        group.MapGet("/{id}", async (IGamesRepository _repository, int id) =>
        {
            var game = await _repository.GetAsync(id);

            if (game is null) return Results.NotFound();

            return Results.Ok(game.AsDtoV1());
        })
        .WithName(GetGameV1EndpointName)
        .RequireAuthorization(Policies.ReadAccess)
        .MapToApiVersion(1.0);

        // V2 GET ENDPOINTS
        group.MapGet("/", async (
            IGamesRepository _repository,
            HttpContext _http,
            [AsParameters] GetGamesDtoV2 request) =>
        {
            var totalCount = await _repository.CountAsync();
            _http.Response.AddPaginationHeader(totalCount, request.PageSize);

            return Results.Ok((await _repository.GetAllAsync(request.PageNumber, request.PageSize))
                                                .Select(game => game.AsDtoV2()));
        })
        .MapToApiVersion(2.0); ;


        group.MapGet("/{id}", async (IGamesRepository _repository, int id) =>
        {
            var game = await _repository.GetAsync(id);

            if (game is null) return Results.NotFound();

            return Results.Ok(game.AsDtoV2());
        })
        .WithName(GetGameV2EndpointName)
        .RequireAuthorization(Policies.ReadAccess)
        .MapToApiVersion(2.0); ;


        group.MapPost("/", async (IGamesRepository _repository, CreateGameDto gameDto) =>
        {
            var game = new Game
            {
                Name = gameDto.Name,
                Genre = gameDto.Genre,
                Price = gameDto.Price,
                ReleaseDate = gameDto.ReleaseDate,
                ImageUri = gameDto.ImageUri
            };
            await _repository.CreateAsync(game);
            return Results.CreatedAtRoute(GetGameV1EndpointName, new { id = game.Id }, game.AsDtoV1());
        })
        .RequireAuthorization(Policies.WriteAccess)
        .MapToApiVersion(1.0);

        group.MapPost("/{id}", async (IGamesRepository _repository, int id, UpdateGameDto updatedGameDto) =>
        {
            var existingGame = await _repository.GetAsync(id);

            if (existingGame is null) return Results.NotFound();

            existingGame.Name = updatedGameDto.Name;
            existingGame.Genre = updatedGameDto.Genre;
            existingGame.Price = updatedGameDto.Price;
            existingGame.ReleaseDate = updatedGameDto.ReleaseDate;
            existingGame.ImageUri = updatedGameDto.ImageUri;

            await _repository.UpdateAsync(existingGame);
            return Results.NoContent();
        })
        .RequireAuthorization(Policies.WriteAccess)
        .MapToApiVersion(1.0);

        group.MapDelete("/{id}", async (IGamesRepository _repository, int id) =>
        {
            var game = await _repository.GetAsync(id);

            if (game is not null)
                await _repository.DeleteAsync(id);

            return Results.NoContent();
        })
        .RequireAuthorization(Policies.WriteAccess)
        .MapToApiVersion(1.0);

        return group;
    }
}