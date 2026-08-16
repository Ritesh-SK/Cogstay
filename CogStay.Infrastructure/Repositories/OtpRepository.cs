using System;
using System.Threading.Tasks;
using MongoDB.Driver;
using CogStay.Application.Contracts.Persistence;
using CogStay.Domain.Entities;
using CogStay.Domain.Enums;
using CogStay.Infrastructure.Data;

namespace CogStay.Infrastructure.Repositories;

public class OtpRepository : IOtpRepository
{
    private readonly MongoDbContext _context;

    public OtpRepository(MongoDbContext context)
    {
        _context = context;
    }

    public async Task CreateAsync(OtpRecord otp)
    {
        await _context.Otps.InsertOneAsync(otp);
    }

    public async Task<OtpRecord?> GetLatestValidOtpAsync(string userId, string target, OtpType type)
    {
        return await _context.Otps
            .Find(o => o.UserId == userId && o.Target.ToLower() == target.ToLower() && o.OtpType == type && !o.IsUsed && o.ExpiresAt > DateTime.UtcNow)
            .SortByDescending(o => o.CreatedAt)
            .FirstOrDefaultAsync();
    }

    public async Task UpdateAsync(OtpRecord otp)
    {
        await _context.Otps.ReplaceOneAsync(o => o.Id == otp.Id, otp);
    }

    public async Task InvalidateExistingOtpsAsync(string userId, string target, OtpType type)
    {
        var update = Builders<OtpRecord>.Update.Set(o => o.IsUsed, true);
        await _context.Otps.UpdateManyAsync(
            o => o.UserId == userId && o.Target.ToLower() == target.ToLower() && o.OtpType == type && !o.IsUsed,
            update);
    }
}
