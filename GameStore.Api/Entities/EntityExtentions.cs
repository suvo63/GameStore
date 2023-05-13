using GameStore.Api.Dtos;

namespace GameStore.Api.Entities;

public static class EntityExtentions
{
    public static GameDto AsDto(this Game game)
    {
        return new GameDto(
            game.Id,
            game.Name,
            game.Genre,
            game.Price,
            game.ReleaseDate,
            game.ImageUri
        );
    }
}