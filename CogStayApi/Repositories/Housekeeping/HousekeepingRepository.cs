using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using CogStayMVC.Data;
using CogStayMVC.Models;
using CogStayMVC.Repositories.Interfaces;
using CogStayMVC.Repositories.Implementations;

namespace CogStayMVC.Repositories.Housekeeping;

public class HousekeepingTaskRepository : Repository<HousekeepingTask>, IHousekeepingTaskRepository
{
    public HousekeepingTaskRepository(HotelDbContext context) : base(context) { }

    public async Task<IEnumerable<HousekeepingTask>> GetTasksWithDetailsAsync()
    {
        return await _dbSet
            .Include(t => t.Room)
            .ToListAsync();
    }

    public async Task<HousekeepingTask?> GetTaskWithDetailsAsync(int id)
    {
        return await _dbSet
            .Include(t => t.Room)
            .FirstOrDefaultAsync(t => t.TaskId == id);
    }

    public async Task<IEnumerable<HousekeepingTask>> GetTasksByRoomIdAsync(int roomId)
    {
        return await _dbSet
            .Include(t => t.Room)
            .Where(t => t.RoomId == roomId)
            .ToListAsync();
    }
}
