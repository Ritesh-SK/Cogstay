using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Driver;
using CogStay.Application.Contracts.Persistence;
using CogStay.Domain.Entities;
using CogStay.Domain.Enums;
using CogStay.Infrastructure.Data;

namespace CogStay.Infrastructure.Repositories;

public class RoomRepository : IRoomRepository
{
    private readonly MongoDbContext _context;

    public RoomRepository(MongoDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Room>> GetAllAsync()
    {
        return await _context.Rooms.Find(_ => true).ToListAsync();
    }

    public async Task<IEnumerable<Room>> GetAvailableRoomsAsync()
    {
        return await _context.Rooms.Find(r => r.Status == RoomStatus.Available).ToListAsync();
    }

    public async Task<Room?> GetByIdAsync(int id)
    {
        return await _context.Rooms.Find(r => r.RoomId == id).FirstOrDefaultAsync();
    }

    public async Task<Room?> GetByRoomNumberAsync(string roomNumber)
    {
        return await _context.Rooms.Find(r => r.RoomNumber == roomNumber).FirstOrDefaultAsync();
    }

    public async Task<Room> CreateAsync(Room room)
    {
        await _context.Rooms.InsertOneAsync(room);
        return room;
    }

    public async Task UpdateAsync(Room room)
    {
        await _context.Rooms.ReplaceOneAsync(r => r.Id == room.Id, room);
    }

    public async Task DeleteAsync(int id)
    {
        await _context.Rooms.DeleteOneAsync(r => r.RoomId == id);
    }

    public async Task<int> GetNextRoomIdAsync()
    {
        var maxRoom = await _context.Rooms
            .Find(_ => true)
            .SortByDescending(r => r.RoomId)
            .FirstOrDefaultAsync();

        return maxRoom == null ? 1 : maxRoom.RoomId + 1;
    }
}
