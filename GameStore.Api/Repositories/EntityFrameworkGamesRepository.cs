using GameStore.Api.Data;
using GameStore.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace GameStore.Api.Repositories;

public class EntityFrameworkGamesRepository : IGamesRepository
{
    private readonly GameStoreContext _dbContext;
    private readonly ILogger<EntityFrameworkGamesRepository> _logger;

    public EntityFrameworkGamesRepository(GameStoreContext dbContext, ILogger<EntityFrameworkGamesRepository> logger)
    {
        _logger = logger;
        _dbContext = dbContext;
    }

    public async Task<IEnumerable<Game>> GetAllAsync(int pageNumber, int pageSize)
    {
        var skipCount = (pageNumber - 1) * pageSize;
        return await _dbContext.Games
                                .OrderBy(game => game.Id)
                                .Skip(skipCount)
                                .Take(pageSize)
                                .AsNoTracking().ToListAsync();
    }
    public async Task<Game?> GetAsync(int id)
    {
        return await _dbContext.Games.FindAsync(id);
    }
    public async Task CreateAsync(Game game)
    {
        _dbContext.Games.Add(game);
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Created game {Name} with price {Price}", game.Name, game.Price);
    }
    public async Task UpdateAsync(Game updatedGame)
    {
        _dbContext.Games.Update(updatedGame);
        await _dbContext.SaveChangesAsync();
    }
    public async Task DeleteAsync(int id)
    {
        await _dbContext.Games.Where(game => game.Id == id)
                        .ExecuteDeleteAsync();
    }

    public async Task<int> CountAsync()
    {
        return await _dbContext.Games.CountAsync();
    }
}