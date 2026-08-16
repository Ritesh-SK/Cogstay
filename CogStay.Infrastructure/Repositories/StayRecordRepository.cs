using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Driver;
using CogStay.Application.Contracts.Persistence;
using CogStay.Domain.Entities;
using CogStay.Infrastructure.Data;

namespace CogStay.Infrastructure.Repositories;

public class StayRecordRepository : IStayRecordRepository
{
    private readonly MongoDbContext _context;

    public StayRecordRepository(MongoDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<StayRecord>> GetAllAsync()
    {
        return await _context.StayRecords.Find(_ => true).ToListAsync();
    }

    public async Task<StayRecord?> GetByIdAsync(int id)
    {
        return await _context.StayRecords.Find(s => s.StayId == id).FirstOrDefaultAsync();
    }

    public async Task<StayRecord?> GetByReservationIdAsync(int reservationId)
    {
        return await _context.StayRecords.Find(s => s.ReservationId == reservationId).FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<StayRecord>> GetByGuestIdAsync(int guestId)
    {
        return await _context.StayRecords.Find(s => s.GuestId == guestId).ToListAsync();
    }

    public async Task<StayRecord> CreateAsync(StayRecord stayRecord)
    {
        await _context.StayRecords.InsertOneAsync(stayRecord);
        return stayRecord;
    }

    public async Task UpdateAsync(StayRecord stayRecord)
    {
        await _context.StayRecords.ReplaceOneAsync(s => s.Id == stayRecord.Id, stayRecord);
    }

    public async Task DeleteAsync(int id)
    {
        await _context.StayRecords.DeleteOneAsync(s => s.StayId == id);
    }

    public async Task<int> GetNextStayIdAsync()
    {
        var maxStay = await _context.StayRecords
            .Find(_ => true)
            .SortByDescending(s => s.StayId)
            .FirstOrDefaultAsync();

        return maxStay == null ? 1 : maxStay.StayId + 1;
    }
}
