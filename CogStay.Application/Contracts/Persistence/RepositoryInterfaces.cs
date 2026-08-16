using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CogStay.Domain.Entities;
using CogStay.Domain.Enums;
using TaskStatus = CogStay.Domain.Enums.TaskStatus;

namespace CogStay.Application.Contracts.Persistence;

public interface IGuestRepository
{
    Task<IEnumerable<Guest>> GetAllAsync();
    Task<Guest?> GetByIdAsync(int id);
    Task<Guest?> GetByEmailAsync(string email);
    Task<Guest?> GetByPhoneAsync(string phone);
    Task<Guest> CreateAsync(Guest guest);
    Task UpdateAsync(Guest guest);
    Task DeleteAsync(int id);
    Task<int> GetNextGuestIdAsync();
}

public interface IStaffRepository
{
    Task<IEnumerable<Staff>> GetAllAsync();
    Task<Staff?> GetByIdAsync(int id);
    Task<Staff?> GetByEmailAsync(string email);
    Task<Staff> CreateAsync(Staff staff);
    Task UpdateAsync(Staff staff);
    Task DeleteAsync(int id);
    Task<int> GetNextStaffIdAsync();
}

public interface IRoomRepository
{
    Task<IEnumerable<Room>> GetAllAsync();
    Task<IEnumerable<Room>> GetAvailableRoomsAsync();
    Task<Room?> GetByIdAsync(int id);
    Task<Room?> GetByRoomNumberAsync(string roomNumber);
    Task<Room> CreateAsync(Room room);
    Task UpdateAsync(Room room);
    Task DeleteAsync(int id);
    Task<int> GetNextRoomIdAsync();
}

public interface IReservationRepository
{
    Task<IEnumerable<Reservation>> GetAllAsync();
    Task<Reservation?> GetByIdAsync(int id);
    Task<IEnumerable<Reservation>> GetByGuestIdAsync(int guestId);
    Task<Reservation> CreateAsync(Reservation reservation);
    Task UpdateAsync(Reservation reservation);
    Task DeleteAsync(int id);
    Task<int> GetNextReservationIdAsync();
    Task<bool> HasConflictingReservationAsync(int roomId, DateTime checkIn, DateTime checkOut, int? excludeReservationId = null);
}

public interface IStayRecordRepository
{
    Task<IEnumerable<StayRecord>> GetAllAsync();
    Task<StayRecord?> GetByIdAsync(int id);
    Task<StayRecord?> GetByReservationIdAsync(int reservationId);
    Task<IEnumerable<StayRecord>> GetByGuestIdAsync(int guestId);
    Task<StayRecord> CreateAsync(StayRecord stayRecord);
    Task UpdateAsync(StayRecord stayRecord);
    Task DeleteAsync(int id);
    Task<int> GetNextStayIdAsync();
}

public interface IBillingRepository
{
    Task<IEnumerable<Billing>> GetAllAsync();
    Task<Billing?> GetByIdAsync(int id);
    Task<Billing?> GetByStayIdAsync(int stayId);
    Task<IEnumerable<Billing>> GetByGuestIdAsync(int guestId);
    Task<Billing> CreateAsync(Billing billing);
    Task UpdateAsync(Billing billing);
    Task DeleteAsync(int id);
    Task<int> GetNextBillIdAsync();
}

public interface IHousekeepingTaskRepository
{
    Task<IEnumerable<HousekeepingTask>> GetAllAsync();
    Task<HousekeepingTask?> GetByIdAsync(int id);
    Task<IEnumerable<HousekeepingTask>> GetByRoomIdAsync(int roomId);
    Task<HousekeepingTask> CreateAsync(HousekeepingTask task);
    Task UpdateAsync(HousekeepingTask task);
    Task DeleteAsync(int id);
    Task<int> GetNextTaskIdAsync();
}

public interface IFeedbackRepository
{
    Task<IEnumerable<Feedback>> GetAllAsync();
    Task<Feedback?> GetByIdAsync(int id);
    Task<IEnumerable<Feedback>> GetByGuestIdAsync(int guestId);
    Task<Feedback> CreateAsync(Feedback feedback);
    Task DeleteAsync(int id);
    Task<int> GetNextFeedbackIdAsync();
}

public interface IOtpRepository
{
    Task CreateAsync(OtpRecord otp);
    Task<OtpRecord?> GetLatestValidOtpAsync(string userId, string target, OtpType type);
    Task UpdateAsync(OtpRecord otp);
    Task InvalidateExistingOtpsAsync(string userId, string target, OtpType type);
}

public interface IRefreshTokenRepository
{
    Task CreateAsync(RefreshToken token);
    Task<RefreshToken?> GetByTokenAsync(string token);
    Task RevokeTokenAsync(string token, string? replacedByToken = null);
}
