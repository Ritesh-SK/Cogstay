using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Driver;
using CogStay.Application.Contracts.Persistence;
using CogStay.Domain.Entities;
using CogStay.Infrastructure.Data;

namespace CogStay.Infrastructure.Repositories;

public class BillingRepository : IBillingRepository
{
    private readonly MongoDbContext _context;

    public BillingRepository(MongoDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Billing>> GetAllAsync()
    {
        return await _context.Billings.Find(_ => true).ToListAsync();
    }

    public async Task<Billing?> GetByIdAsync(int id)
    {
        return await _context.Billings.Find(b => b.BillId == id).FirstOrDefaultAsync();
    }

    public async Task<Billing?> GetByStayIdAsync(int stayId)
    {
        return await _context.Billings.Find(b => b.StayId == stayId).FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<Billing>> GetByGuestIdAsync(int guestId)
    {
        return await _context.Billings.Find(b => b.GuestId == guestId).ToListAsync();
    }

    public async Task<Billing> CreateAsync(Billing billing)
    {
        await _context.Billings.InsertOneAsync(billing);
        return billing;
    }

    public async Task UpdateAsync(Billing billing)
    {
        await _context.Billings.ReplaceOneAsync(b => b.Id == billing.Id, billing);
    }

    public async Task DeleteAsync(int id)
    {
        await _context.Billings.DeleteOneAsync(b => b.BillId == id);
    }

    public async Task<int> GetNextBillIdAsync()
    {
        var maxBill = await _context.Billings
            .Find(_ => true)
            .SortByDescending(b => b.BillId)
            .FirstOrDefaultAsync();

        return maxBill == null ? 1 : maxBill.BillId + 1;
    }
}
