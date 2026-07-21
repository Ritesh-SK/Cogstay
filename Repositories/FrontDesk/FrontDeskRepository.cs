using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using CogStayMVC.Data;
using CogStayMVC.Enums;
using CogStayMVC.Models;
using CogStayMVC.Repositories.Interfaces;
using CogStayMVC.Repositories.Implementations;

namespace CogStayMVC.Repositories.FrontDesk;

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
