using System.Diagnostics;
using GameStore.Api.Authorization;
using GameStore.Api.Dtos;
using GameStore.Api.Entities;
using GameStore.Api.Repositories;

namespace GameStore.Api.Endpoints;

public static class GamesEndpoints
{
    const string GetGameEndpointName = "GetGame";

    public static RouteGroupBuilder MapGamesEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/games")
                       .WithParameterValidation();

        group.MapGet("/", async (IGamesRepository _repository) =>
        {
            return Results.Ok((await _repository.GetAllAsync()).Select(game => game.AsDto()));
        });


        group.MapGet("/{id}", async (IGamesRepository _repository, int id) =>
        {
            var game = await _repository.GetAsync(id);

            if (game is null) return Results.NotFound();

            return Results.Ok(game.AsDto());
        })
        .WithName(GetGameEndpointName)
        .RequireAuthorization(Policies.ReadAccess);


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
            return Results.CreatedAtRoute(GetGameEndpointName, new { id = game.Id }, game.AsDto());
        })
        .RequireAuthorization(Policies.WriteAccess);
        // .RequireAuthorization(policy =>
        // {
        //     policy.RequireRole("Admin");
        // });

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
        .RequireAuthorization(Policies.WriteAccess);

        group.MapDelete("/{id}", async (IGamesRepository _repository, int id) =>
        {
            var game = await _repository.GetAsync(id);

            if (game is not null)
                await _repository.DeleteAsync(id);

            return Results.NoContent();
        })
        .RequireAuthorization(Policies.WriteAccess);

        return group;
    }
}