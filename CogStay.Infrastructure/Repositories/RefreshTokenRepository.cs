using System.Threading.Tasks;
using MongoDB.Driver;
using CogStay.Application.Contracts.Persistence;
using CogStay.Domain.Entities;
using CogStay.Infrastructure.Data;

namespace CogStay.Infrastructure.Repositories;

public class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly MongoDbContext _context;

    public RefreshTokenRepository(MongoDbContext context)
    {
        _context = context;
    }

    public async Task CreateAsync(RefreshToken token)
    {
        await _context.RefreshTokens.InsertOneAsync(token);
    }

    public async Task<RefreshToken?> GetByTokenAsync(string token)
    {
        return await _context.RefreshTokens.Find(rt => rt.Token == token).FirstOrDefaultAsync();
    }

    public async Task RevokeTokenAsync(string token, string? replacedByToken = null)
    {
        var update = Builders<RefreshToken>.Update
            .Set(rt => rt.IsRevoked, true)
            .Set(rt => rt.ReplacedByToken, replacedByToken);

        await _context.RefreshTokens.UpdateOneAsync(rt => rt.Token == token, update);
    }
}
