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

        group.MapGet("/", (IGamesRepository _repository) =>
            _repository.GetAll().Select(game => game.AsDto()));

        group.MapGet("/{id}", (IGamesRepository _repository, int id) =>
        {
            var game = _repository.Get(id);

            if (game is null) return Results.NotFound();

            return Results.Ok(game.AsDto());
        })
        .WithName(GetGameEndpointName);

        group.MapPost("/", (IGamesRepository _repository, CreateGameDto gameDto) =>
        {
            var game = new Game
            {
                Name = gameDto.Name,
                Genre = gameDto.Genre,
                Price = gameDto.Price,
                ReleaseDate = gameDto.ReleaseDate,
                ImageUri = gameDto.ImageUri
            };
            _repository.Create(game);
            return Results.CreatedAtRoute(GetGameEndpointName, new { id = game.Id }, game.AsDto());
        });

        group.MapPost("/{id}", (IGamesRepository _repository, int id, UpdateGameDto updatedGameDto) =>
        {
            var existingGame = _repository.Get(id);

            if (existingGame is null) return Results.NotFound();

            existingGame.Name = updatedGameDto.Name;
            existingGame.Genre = updatedGameDto.Genre;
            existingGame.Price = updatedGameDto.Price;
            existingGame.ReleaseDate = updatedGameDto.ReleaseDate;
            existingGame.ImageUri = updatedGameDto.ImageUri;

            _repository.Update(existingGame);
            return Results.NoContent();
        });

        group.MapDelete("/{id}", (IGamesRepository _repository, int id) =>
        {
            var game = _repository.Get(id);

            if (game is not null)
                _repository.Delete(id);

            return Results.NoContent();
        });

        return group;
    }
}