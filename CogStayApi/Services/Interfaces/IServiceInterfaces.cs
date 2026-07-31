using System.Collections.Generic;
using System.Threading.Tasks;
using CogStayMVC.DTOs;
using CogStayMVC.Enums;
using TaskStatus = CogStayMVC.Enums.TaskStatus;


namespace CogStayMVC.Services.Interfaces;

public interface IGuestService
{
    Task<IEnumerable<GuestResponseDTO>> GetAllGuestsAsync();
    Task<GuestResponseDTO?> GetGuestByIdAsync(int id);
    Task<GuestResponseDTO?> GetGuestByEmailAsync(string email);
    Task<GuestResponseDTO> RegisterGuestAsync(CreateGuestDTO dto);
    Task<GuestResponseDTO?> ValidateGuestLoginAsync(GuestLoginDTO dto);
    Task UpdateGuestAsync(UpdateGuestDTO dto);
    Task DeleteGuestAsync(int id);
}

public interface IRoomService
{
    Task<IEnumerable<RoomResponseDTO>> GetAllRoomsAsync();
    Task<IEnumerable<RoomResponseDTO>> GetAvailableRoomsAsync();
    Task<RoomResponseDTO?> GetRoomByIdAsync(int id);
    Task<RoomResponseDTO> CreateRoomAsync(CreateRoomDTO dto);
    Task UpdateRoomAsync(UpdateRoomDTO dto);
    Task UpdateRoomStatusAsync(int roomId, RoomStatus status);
    Task DeleteRoomAsync(int id);
}

public interface IReservationService
{
    Task<IEnumerable<ReservationResponseDTO>> GetAllReservationsAsync();
    Task<ReservationResponseDTO?> GetReservationByIdAsync(int id);
    Task<IEnumerable<ReservationResponseDTO>> GetReservationsByGuestAsync(int guestId);
    Task<ReservationResponseDTO> BookRoomAsync(CreateReservationDTO dto);
    Task UpdateReservationAsync(UpdateReservationDTO dto);
    Task CancelReservationAsync(int reservationId);
    Task DeleteReservationAsync(int id);
}

public interface ICheckInService
{
    Task<IEnumerable<StayRecordResponseDTO>> GetAllStaysAsync();
    Task<StayRecordResponseDTO?> GetStayByIdAsync(int id);
    Task<StayRecordResponseDTO?> GetStayByReservationIdAsync(int reservationId);
    Task<StayRecordResponseDTO> CheckInGuestAsync(CreateCheckInDTO dto);
    Task RequestCheckOutAsync(int stayId);
    Task CompleteCheckOutAsync(int stayId);
    Task DeleteStayAsync(int id);
}

public interface IBillingService
{
    Task<IEnumerable<BillingResponseDTO>> GetAllBillsAsync();
    Task<BillingResponseDTO?> GetBillByIdAsync(int id);
    Task<BillingResponseDTO?> GetBillByStayIdAsync(int stayId);
    Task<BillingResponseDTO> GenerateBillForStayAsync(int stayId, string? remarks = null);
    Task<BillingResponseDTO> CreateBillAsync(CreateBillDTO dto);
    Task ProcessPaymentAsync(ProcessPaymentDTO dto);
    Task DeleteBillAsync(int id);
}

public interface IHousekeepingService
{
    Task<IEnumerable<HousekeepingTaskResponseDTO>> GetAllTasksAsync();
    Task<HousekeepingTaskResponseDTO?> GetTaskByIdAsync(int id);
    Task<IEnumerable<HousekeepingTaskResponseDTO>> GetTasksByRoomIdAsync(int roomId);
    Task<HousekeepingTaskResponseDTO> CreateTaskAsync(CreateHousekeepingTaskDTO dto);
    Task UpdateTaskStatusAsync(UpdateTaskStatusDTO dto);
    Task DeleteTaskAsync(int id);
}

public interface IStaffService
{
    Task<IEnumerable<StaffResponseDTO>> GetAllStaffAsync();
    Task<StaffResponseDTO?> GetStaffByIdAsync(int id);
    Task<StaffResponseDTO> CreateStaffAsync(CreateStaffDTO dto);
    Task UpdateStaffAsync(UpdateStaffDTO dto);
    Task DeleteStaffAsync(int id);
    Task<StaffResponseDTO?> ValidateStaffLoginAsync(StaffLoginDTO dto);
}

public interface IFeedbackService
{
    Task<IEnumerable<FeedbackResponseDTO>> GetAllFeedbacksAsync();
    Task<FeedbackResponseDTO?> GetFeedbackByIdAsync(int id);
    Task<FeedbackResponseDTO> SubmitFeedbackAsync(CreateFeedbackDTO dto);
    Task DeleteFeedbackAsync(int id);
}
