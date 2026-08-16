using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Driver;
using CogStay.Application.Contracts.Persistence;
using CogStay.Domain.Entities;
using CogStay.Infrastructure.Data;

namespace CogStay.Infrastructure.Repositories;

public class StaffRepository : IStaffRepository
{
    private readonly MongoDbContext _context;

    public StaffRepository(MongoDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Staff>> GetAllAsync()
    {
        return await _context.Staff.Find(_ => true).ToListAsync();
    }

    public async Task<Staff?> GetByIdAsync(int id)
    {
        return await _context.Staff.Find(s => s.StaffId == id).FirstOrDefaultAsync();
    }

    public async Task<Staff?> GetByEmailAsync(string email)
    {
        if (string.IsNullOrWhiteSpace(email)) return null;
        return await _context.Staff.Find(s => s.Email.ToLower() == email.ToLower()).FirstOrDefaultAsync();
    }

    public async Task<Staff> CreateAsync(Staff staff)
    {
        await _context.Staff.InsertOneAsync(staff);
        return staff;
    }

    public async Task UpdateAsync(Staff staff)
    {
        await _context.Staff.ReplaceOneAsync(s => s.Id == staff.Id, staff);
    }

    public async Task DeleteAsync(int id)
    {
        await _context.Staff.DeleteOneAsync(s => s.StaffId == id);
    }

    public async Task<int> GetNextStaffIdAsync()
    {
        var maxStaff = await _context.Staff
            .Find(_ => true)
            .SortByDescending(s => s.StaffId)
            .FirstOrDefaultAsync();

        return maxStaff == null ? 1 : maxStaff.StaffId + 1;
    }
}
