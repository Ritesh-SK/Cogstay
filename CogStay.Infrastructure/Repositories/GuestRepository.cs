using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Driver;
using CogStay.Application.Contracts.Persistence;
using CogStay.Domain.Entities;
using CogStay.Infrastructure.Data;

namespace CogStay.Infrastructure.Repositories;

public class GuestRepository : IGuestRepository
{
    private readonly MongoDbContext _context;

    public GuestRepository(MongoDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Guest>> GetAllAsync()
    {
        return await _context.Guests.Find(_ => true).ToListAsync();
    }

    public async Task<Guest?> GetByIdAsync(int id)
    {
        return await _context.Guests.Find(g => g.GuestId == id).FirstOrDefaultAsync();
    }

    public async Task<Guest?> GetByEmailAsync(string email)
    {
        if (string.IsNullOrWhiteSpace(email)) return null;
        return await _context.Guests.Find(g => g.Email.ToLower() == email.ToLower()).FirstOrDefaultAsync();
    }

    public async Task<Guest?> GetByPhoneAsync(string phone)
    {
        if (string.IsNullOrWhiteSpace(phone)) return null;
        return await _context.Guests.Find(g => g.PhoneNumber == phone).FirstOrDefaultAsync();
    }

    public async Task<Guest> CreateAsync(Guest guest)
    {
        await _context.Guests.InsertOneAsync(guest);
        return guest;
    }

    public async Task UpdateAsync(Guest guest)
    {
        await _context.Guests.ReplaceOneAsync(g => g.Id == guest.Id, guest);
    }

    public async Task DeleteAsync(int id)
    {
        await _context.Guests.DeleteOneAsync(g => g.GuestId == id);
    }

    public async Task<int> GetNextGuestIdAsync()
    {
        var maxGuest = await _context.Guests
            .Find(_ => true)
            .SortByDescending(g => g.GuestId)
            .FirstOrDefaultAsync();

        return maxGuest == null ? 1 : maxGuest.GuestId + 1;
    }
}
