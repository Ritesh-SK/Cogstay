using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using CogStayMVC.Data;
using CogStayMVC.Enums;
using CogStayMVC.Models;
using CogStayMVC.Repositories.Interfaces;

namespace CogStayMVC.Repositories.Implementations;

public class GuestRepository : Repository<Guest>, IGuestRepository
{
    public GuestRepository(HotelDbContext context) : base(context) { }

    public async Task<Guest?> GetByEmailAsync(string email)
    {
        return await _dbSet.FirstOrDefaultAsync(g => g.Email == email);
    }
}

public class RoomRepository : Repository<Room>, IRoomRepository
{
    public RoomRepository(HotelDbContext context) : base(context) { }

    public async Task<Room?> GetByRoomNumberAsync(string roomNumber)
    {
        return await _dbSet.FirstOrDefaultAsync(r => r.RoomNumber == roomNumber);
    }

    public async Task<IEnumerable<Room>> GetRoomsByStatusAsync(RoomStatus status)
    {
        return await _dbSet.Where(r => r.Status == status).ToListAsync();
    }
}

public class ReservationRepository : Repository<Reservation>, IReservationRepository
{
    public ReservationRepository(HotelDbContext context) : base(context) { }

    public async Task<IEnumerable<Reservation>> GetReservationsWithDetailsAsync()
    {
        return await _dbSet
            .Include(r => r.Guest)
            .Include(r => r.Room)
            .Include(r => r.StayRecord)
            .ToListAsync();
    }

    public async Task<Reservation?> GetReservationWithDetailsAsync(int id)
    {
        return await _dbSet
            .Include(r => r.Guest)
            .Include(r => r.Room)
            .Include(r => r.StayRecord)
            .FirstOrDefaultAsync(r => r.ReservationId == id);
    }

    public async Task<IEnumerable<Reservation>> GetReservationsByGuestAsync(int guestId)
    {
        return await _dbSet
            .Include(r => r.Room)
            .Include(r => r.StayRecord)
            .Where(r => r.GuestId == guestId)
            .ToListAsync();
    }
}

public class StayRecordRepository : Repository<StayRecord>, IStayRecordRepository
{
    public StayRecordRepository(HotelDbContext context) : base(context) { }

    public async Task<IEnumerable<StayRecord>> GetStayRecordsWithDetailsAsync()
    {
        return await _dbSet
            .Include(s => s.Guest)
            .Include(s => s.Reservation)
                .ThenInclude(r => r.Room)
            .Include(s => s.Billing)
            .ToListAsync();
    }

    public async Task<StayRecord?> GetStayRecordWithDetailsAsync(int id)
    {
        return await _dbSet
            .Include(s => s.Guest)
            .Include(s => s.Reservation)
                .ThenInclude(r => r.Room)
            .Include(s => s.Billing)
            .FirstOrDefaultAsync(s => s.StayId == id);
    }

    public async Task<StayRecord?> GetStayRecordByReservationAsync(int reservationId)
    {
        return await _dbSet
            .Include(s => s.Guest)
            .Include(s => s.Reservation)
                .ThenInclude(r => r.Room)
            .Include(s => s.Billing)
            .FirstOrDefaultAsync(s => s.ReservationId == reservationId);
    }
}

public class BillingRepository : Repository<Billing>, IBillingRepository
{
    public BillingRepository(HotelDbContext context) : base(context) { }

    public async Task<IEnumerable<Billing>> GetBillingsWithDetailsAsync()
    {
        return await _dbSet
            .Include(b => b.StayRecord)
                .ThenInclude(s => s.Guest)
            .Include(b => b.StayRecord)
                .ThenInclude(s => s.Reservation)
                    .ThenInclude(r => r.Room)
            .ToListAsync();
    }

    public async Task<Billing?> GetBillingWithDetailsAsync(int id)
    {
        return await _dbSet
            .Include(b => b.StayRecord)
                .ThenInclude(s => s.Guest)
            .Include(b => b.StayRecord)
                .ThenInclude(s => s.Reservation)
                    .ThenInclude(r => r.Room)
            .FirstOrDefaultAsync(b => b.BillId == id);
    }

    public async Task<Billing?> GetBillingByStayIdAsync(int stayId)
    {
        return await _dbSet
            .Include(b => b.StayRecord)
                .ThenInclude(s => s.Guest)
            .Include(b => b.StayRecord)
                .ThenInclude(s => s.Reservation)
                    .ThenInclude(r => r.Room)
            .FirstOrDefaultAsync(b => b.StayId == stayId);
    }
}

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

public class StaffRepository : Repository<Staff>, IStaffRepository
{
    public StaffRepository(HotelDbContext context) : base(context) { }

    public async Task<Staff?> GetByEmailAsync(string email)
    {
        return await _dbSet.FirstOrDefaultAsync(s => s.Email == email);
    }
}

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
