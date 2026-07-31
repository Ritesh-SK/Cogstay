using System.Collections.Generic;
using System.Threading.Tasks;
using CogStayMVC.Enums;
using CogStayMVC.Models;

namespace CogStayMVC.Repositories.Interfaces;

public interface IGuestRepository : IRepository<Guest>
{
    Task<Guest?> GetByEmailAsync(string email);
}

public interface IRoomRepository : IRepository<Room>
{
    Task<Room?> GetByRoomNumberAsync(string roomNumber);
    Task<IEnumerable<Room>> GetRoomsByStatusAsync(RoomStatus status);
}

public interface IReservationRepository : IRepository<Reservation>
{
    Task<IEnumerable<Reservation>> GetReservationsWithDetailsAsync();
    Task<Reservation?> GetReservationWithDetailsAsync(int id);
    Task<IEnumerable<Reservation>> GetReservationsByGuestAsync(int guestId);
}

public interface IStayRecordRepository : IRepository<StayRecord>
{
    Task<IEnumerable<StayRecord>> GetStayRecordsWithDetailsAsync();
    Task<StayRecord?> GetStayRecordWithDetailsAsync(int id);
    Task<StayRecord?> GetStayRecordByReservationAsync(int reservationId);
}

public interface IBillingRepository : IRepository<Billing>
{
    Task<IEnumerable<Billing>> GetBillingsWithDetailsAsync();
    Task<Billing?> GetBillingWithDetailsAsync(int id);
    Task<Billing?> GetBillingByStayIdAsync(int stayId);
}

public interface IHousekeepingTaskRepository : IRepository<HousekeepingTask>
{
    Task<IEnumerable<HousekeepingTask>> GetTasksWithDetailsAsync();
    Task<HousekeepingTask?> GetTaskWithDetailsAsync(int id);
    Task<IEnumerable<HousekeepingTask>> GetTasksByRoomIdAsync(int roomId);
}

public interface IStaffRepository : IRepository<Staff>
{
    Task<Staff?> GetByEmailAsync(string email);
}

public interface IFeedbackRepository : IRepository<Feedback>
{
    Task<IEnumerable<Feedback>> GetFeedbacksWithDetailsAsync();
}
