using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using CogStayMVC.Data;
using CogStayMVC.Models;
using CogStayMVC.Repositories.Interfaces;
using CogStayMVC.Repositories.Implementations;

namespace CogStayMVC.Repositories.Manager;

public class FeedbackRepository : Repository<Feedback>, IFeedbackRepository
{
    public FeedbackRepository(HotelDbContext context) : base(context) { }

    public async Task<IEnumerable<Feedback>> GetFeedbacksWithDetailsAsync()
    {
        return await _dbSet
            .Include(f => f.Guest)
            .Include(f => f.Reservation)
                .ThenInclude(r => r!.Room)
            .OrderByDescending(f => f.CreatedAt)
            .ToListAsync();
    }
}
