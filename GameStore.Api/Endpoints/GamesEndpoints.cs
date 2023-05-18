using GameStore.Api.Authorization;
using GameStore.Api.Dtos;
using GameStore.Api.Entities;
using GameStore.Api.Repositories;
using Microsoft.AspNetCore.Http.HttpResults;

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
                        .WithParameterValidation()
                        .WithOpenApi()
                        .WithTags("Games");

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

            var totalCount = await _repository.CountAsync(request.filter);
            _http.Response.AddPaginationHeader(totalCount, request.PageSize);

            return Results.Ok((await _repository.GetAllAsync(request.PageNumber, request.PageSize, request.filter))
                                                .Select(game => game.AsDtoV1()));
        })
        .MapToApiVersion(1.0)
        .WithSummary("Gets all games")
        .WithDescription("Gets all available games and allow filtering and pagination");

        group.MapGet("/{id}", async Task<Results<Ok<GameDtoV1>, NotFound>> (IGamesRepository _repository, int id) =>
        {
            var game = await _repository.GetAsync(id);

            if (game is null) return TypedResults.NotFound();

            return TypedResults.Ok(game.AsDtoV1());
        })
        .WithName(GetGameV1EndpointName)
        .RequireAuthorization(Policies.ReadAccess)
        .MapToApiVersion(1.0)
        .WithSummary("Gets a game by id")
        .WithDescription("Gets the game that has the specified id");

        // V2 GET ENDPOINTS
        group.MapGet("/", async (
            IGamesRepository _repository,
            HttpContext _http,
            [AsParameters] GetGamesDtoV2 request) =>
        {
            var totalCount = await _repository.CountAsync(request.filter);
            _http.Response.AddPaginationHeader(totalCount, request.PageSize);

            return Results.Ok((await _repository.GetAllAsync(request.PageNumber, request.PageSize, request.filter))
                                                .Select(game => game.AsDtoV2()));
        })
        .MapToApiVersion(2.0)
        .WithSummary("Gets all games")
        .WithDescription("Gets all available games and allow filtering and pagination");


        group.MapGet("/{id}", async Task<Results<Ok<GameDtoV2>, NotFound>> (IGamesRepository _repository, int id) =>
        {
            var game = await _repository.GetAsync(id);

            if (game is null) return TypedResults.NotFound();

            return TypedResults.Ok(game.AsDtoV2());
        })
        .WithName(GetGameV2EndpointName)
        .RequireAuthorization(Policies.ReadAccess)
        .MapToApiVersion(2.0)
        .WithSummary("Gets a game by id")
        .WithDescription("Gets the game that has the specified id");


        group.MapPost("/", async Task<CreatedAtRoute<GameDtoV1>> (IGamesRepository _repository, CreateGameDto gameDto) =>
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
            return TypedResults.CreatedAtRoute(game.AsDtoV1(), GetGameV1EndpointName, new { id = game.Id });
        })
        .RequireAuthorization(Policies.WriteAccess)
        .MapToApiVersion(1.0)
        .WithSummary("Creates a new game")
        .WithDescription("Creates a new game with the specified properties");

        group.MapPut("/{id}", async Task<Results<NoContent, NotFound>> (IGamesRepository _repository, int id, UpdateGameDto updatedGameDto) =>
        {
            var existingGame = await _repository.GetAsync(id);

            if (existingGame is null) return TypedResults.NotFound();

            existingGame.Name = updatedGameDto.Name;
            existingGame.Genre = updatedGameDto.Genre;
            existingGame.Price = updatedGameDto.Price;
            existingGame.ReleaseDate = updatedGameDto.ReleaseDate;
            existingGame.ImageUri = updatedGameDto.ImageUri;

            await _repository.UpdateAsync(existingGame);
            return TypedResults.NoContent();
        })
        .RequireAuthorization(Policies.WriteAccess)
        .MapToApiVersion(1.0)
        .WithSummary("Updates the game")
        .WithDescription("Updates all game properties for the game that has the specified id");

        group.MapDelete("/{id}", async (IGamesRepository _repository, int id) =>
        {
            var game = await _repository.GetAsync(id);

            if (game is not null)
                await _repository.DeleteAsync(id);

            return TypedResults.NoContent();
        })
        .RequireAuthorization(Policies.WriteAccess)
        .MapToApiVersion(1.0)
        .WithSummary("Deletes a game")
        .WithDescription("Deletes the game that has the specified id");

        return group;
    }
}