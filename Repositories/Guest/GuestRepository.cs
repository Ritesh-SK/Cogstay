using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using CogStayMVC.Data;
using CogStayMVC.Models;
using CogStayMVC.Repositories.Interfaces;
using CogStayMVC.Repositories.Implementations;

namespace CogStayMVC.Repositories.GuestModule;

public class GuestRepository : Repository<Guest>, IGuestRepository
{
    public GuestRepository(HotelDbContext context) : base(context) { }

    public async Task<Guest?> GetByEmailAsync(string email)
    {
        return await _dbSet.FirstOrDefaultAsync(g => g.Email == email);
    }
}
