using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Driver;
using CogStay.Application.Contracts.Persistence;
using CogStay.Domain.Entities;
using CogStay.Infrastructure.Data;

namespace CogStay.Infrastructure.Repositories;

public class HousekeepingTaskRepository : IHousekeepingTaskRepository
{
    private readonly MongoDbContext _context;

    public HousekeepingTaskRepository(MongoDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<HousekeepingTask>> GetAllAsync()
    {
        return await _context.HousekeepingTasks.Find(_ => true).ToListAsync();
    }

    public async Task<HousekeepingTask?> GetByIdAsync(int id)
    {
        return await _context.HousekeepingTasks.Find(t => t.TaskId == id).FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<HousekeepingTask>> GetByRoomIdAsync(int roomId)
    {
        return await _context.HousekeepingTasks.Find(t => t.RoomId == roomId).ToListAsync();
    }

    public async Task<HousekeepingTask> CreateAsync(HousekeepingTask task)
    {
        await _context.HousekeepingTasks.InsertOneAsync(task);
        return task;
    }

    public async Task UpdateAsync(HousekeepingTask task)
    {
        await _context.HousekeepingTasks.ReplaceOneAsync(t => t.Id == task.Id, task);
    }

    public async Task DeleteAsync(int id)
    {
        await _context.HousekeepingTasks.DeleteOneAsync(t => t.TaskId == id);
    }

    public async Task<int> GetNextTaskIdAsync()
    {
        var maxTask = await _context.HousekeepingTasks
            .Find(_ => true)
            .SortByDescending(t => t.TaskId)
            .FirstOrDefaultAsync();

        return maxTask == null ? 1 : maxTask.TaskId + 1;
    }
}
