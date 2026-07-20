using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using CogStayMVC.DTOs;
using CogStayMVC.Enums;
using CogStayMVC.Models;
using CogStayMVC.Repositories.Interfaces;
using CogStayMVC.Services.Interfaces;
using TaskStatus = CogStayMVC.Enums.TaskStatus;


namespace CogStayMVC.Services.Implementations;

public class GuestService : IGuestService
{
    private readonly IGuestRepository _guestRepository;

    public GuestService(IGuestRepository guestRepository)
    {
        _guestRepository = guestRepository;
    }

    public async Task<IEnumerable<GuestResponseDTO>> GetAllGuestsAsync()
    {
        var guests = await _guestRepository.GetAllAsync();
        return guests.Select(MapToDTO);
    }

    public async Task<GuestResponseDTO?> GetGuestByIdAsync(int id)
    {
        var guest = await _guestRepository.GetByIdAsync(id);
        return guest != null ? MapToDTO(guest) : null;
    }

    public async Task<GuestResponseDTO?> GetGuestByEmailAsync(string email)
    {
        var guest = await _guestRepository.GetByEmailAsync(email);
        return guest != null ? MapToDTO(guest) : null;
    }

    public async Task<GuestResponseDTO> RegisterGuestAsync(CreateGuestDTO dto)
    {
        var existing = await _guestRepository.GetByEmailAsync(dto.Email);
        if (existing != null)
        {
            throw new InvalidOperationException("A guest with this email already exists.");
        }

        var guest = new Guest
        {
            FullName = dto.FullName,
            Email = dto.Email,
            PhoneNumber = dto.PhoneNumber,
            Address = dto.Address,
            PasswordHash = HashPassword(dto.Password),
            CreatedAt = DateTime.Now
        };

        await _guestRepository.AddAsync(guest);
        return MapToDTO(guest);
    }

    public async Task<GuestResponseDTO?> ValidateGuestLoginAsync(GuestLoginDTO dto)
    {
        var guest = await _guestRepository.GetByEmailAsync(dto.Email);
        if (guest == null || guest.PasswordHash != HashPassword(dto.Password))
        {
            return null;
        }

        return MapToDTO(guest);
    }

    public async Task UpdateGuestAsync(UpdateGuestDTO dto)
    {
        var guest = await _guestRepository.GetByIdAsync(dto.GuestId);
        if (guest == null)
            throw new KeyNotFoundException("Guest not found.");

        guest.FullName = dto.FullName;
        guest.Email = dto.Email;
        guest.PhoneNumber = dto.PhoneNumber;
        guest.Address = dto.Address;

        await _guestRepository.UpdateAsync(guest);
    }

    public async Task DeleteGuestAsync(int id)
    {
        await _guestRepository.DeleteAsync(id);
    }

    private static GuestResponseDTO MapToDTO(Guest guest) => new()
    {
        GuestId = guest.GuestId,
        FullName = guest.FullName,
        Email = guest.Email,
        PhoneNumber = guest.PhoneNumber,
        Address = guest.Address,
        CreatedAt = guest.CreatedAt
    };

    private static string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(bytes);
    }
}

public class RoomService : IRoomService
{
    private readonly IRoomRepository _roomRepository;

    public RoomService(IRoomRepository roomRepository)
    {
        _roomRepository = roomRepository;
    }

    public async Task<IEnumerable<RoomResponseDTO>> GetAllRoomsAsync()
    {
        var rooms = await _roomRepository.GetAllAsync();
        return rooms.Select(MapToDTO);
    }

    public async Task<IEnumerable<RoomResponseDTO>> GetAvailableRoomsAsync()
    {
        var rooms = await _roomRepository.GetRoomsByStatusAsync(RoomStatus.Available);
        return rooms.Select(MapToDTO);
    }

    public async Task<RoomResponseDTO?> GetRoomByIdAsync(int id)
    {
        var room = await _roomRepository.GetByIdAsync(id);
        return room != null ? MapToDTO(room) : null;
    }

    public async Task<RoomResponseDTO> CreateRoomAsync(CreateRoomDTO dto)
    {
        var existing = await _roomRepository.GetByRoomNumberAsync(dto.RoomNumber);
        if (existing != null)
        {
            throw new InvalidOperationException($"Room number '{dto.RoomNumber}' already exists.");
        }

        var room = new Room
        {
            RoomNumber = dto.RoomNumber,
            RoomType = dto.RoomType,
            PricePerNight = dto.PricePerNight,
            Status = dto.Status
        };

        await _roomRepository.AddAsync(room);
        return MapToDTO(room);
    }

    public async Task UpdateRoomAsync(UpdateRoomDTO dto)
    {
        var room = await _roomRepository.GetByIdAsync(dto.RoomId);
        if (room == null)
            throw new KeyNotFoundException("Room not found.");

        room.RoomNumber = dto.RoomNumber;
        room.RoomType = dto.RoomType;
        room.PricePerNight = dto.PricePerNight;
        room.Status = dto.Status;

        await _roomRepository.UpdateAsync(room);
    }

    public async Task UpdateRoomStatusAsync(int roomId, RoomStatus status)
    {
        var room = await _roomRepository.GetByIdAsync(roomId);
        if (room == null)
            throw new KeyNotFoundException("Room not found.");

        room.Status = status;
        await _roomRepository.UpdateAsync(room);
    }

    public async Task DeleteRoomAsync(int id)
    {
        await _roomRepository.DeleteAsync(id);
    }

    private static RoomResponseDTO MapToDTO(Room room) => new()
    {
        RoomId = room.RoomId,
        RoomNumber = room.RoomNumber,
        RoomType = room.RoomType,
        PricePerNight = room.PricePerNight,
        Status = room.Status
    };
}

public class ReservationService : IReservationService
{
    private readonly IReservationRepository _reservationRepository;
    private readonly IRoomRepository _roomRepository;
    private readonly IGuestRepository _guestRepository;

    public ReservationService(
        IReservationRepository reservationRepository,
        IRoomRepository roomRepository,
        IGuestRepository guestRepository)
    {
        _reservationRepository = reservationRepository;
        _roomRepository = roomRepository;
        _guestRepository = guestRepository;
    }

    public async Task<IEnumerable<ReservationResponseDTO>> GetAllReservationsAsync()
    {
        var resList = await _reservationRepository.GetReservationsWithDetailsAsync();
        return resList.Select(MapToDTO);
    }

    public async Task<ReservationResponseDTO?> GetReservationByIdAsync(int id)
    {
        var res = await _reservationRepository.GetReservationWithDetailsAsync(id);
        return res != null ? MapToDTO(res) : null;
    }

    public async Task<IEnumerable<ReservationResponseDTO>> GetReservationsByGuestAsync(int guestId)
    {
        var resList = await _reservationRepository.GetReservationsByGuestAsync(guestId);
        return resList.Select(MapToDTO);
    }

    public async Task<ReservationResponseDTO> BookRoomAsync(CreateReservationDTO dto)
    {
        if (dto.CheckInDate >= dto.CheckOutDate)
        {
            throw new InvalidOperationException("Check-Out date must be after Check-In date.");
        }

        if (dto.CheckInDate.Date < DateTime.Today)
        {
            throw new InvalidOperationException("Check-In date cannot be in the past.");
        }

        var guest = await _guestRepository.GetByIdAsync(dto.GuestId);
        if (guest == null)
            throw new InvalidOperationException("Guest account not found.");

        var room = await _roomRepository.GetByIdAsync(dto.RoomId);
        if (room == null)
            throw new InvalidOperationException("Selected room not found.");

        if (room.Status != RoomStatus.Available)
        {
            throw new InvalidOperationException($"Room {room.RoomNumber} is currently not available for booking (Status: {room.Status}).");
        }

        // Check overlapping active reservations
        var existingReservations = await _reservationRepository.FindAsync(r =>
            r.RoomId == dto.RoomId &&
            r.ReservationStatus == ReservationStatus.Booked &&
            !(dto.CheckOutDate <= r.CheckInDate || dto.CheckInDate >= r.CheckOutDate));

        if (existingReservations.Any())
        {
            throw new InvalidOperationException($"Room {room.RoomNumber} is already booked for the selected dates.");
        }

        var reservation = new Reservation
        {
            GuestId = dto.GuestId,
            RoomId = dto.RoomId,
            CheckInDate = dto.CheckInDate,
            CheckOutDate = dto.CheckOutDate,
            ReservationStatus = ReservationStatus.Booked
        };

        await _reservationRepository.AddAsync(reservation);

        // Transition room status to Booked
        room.Status = RoomStatus.Booked;
        await _roomRepository.UpdateAsync(room);

        var saved = await _reservationRepository.GetReservationWithDetailsAsync(reservation.ReservationId);
        return MapToDTO(saved ?? reservation);
    }

    public async Task UpdateReservationAsync(UpdateReservationDTO dto)
    {
        var res = await _reservationRepository.GetByIdAsync(dto.ReservationId);
        if (res == null)
            throw new KeyNotFoundException("Reservation not found.");

        res.CheckInDate = dto.CheckInDate;
        res.CheckOutDate = dto.CheckOutDate;
        res.ReservationStatus = dto.ReservationStatus;

        await _reservationRepository.UpdateAsync(res);
    }

    public async Task CancelReservationAsync(int reservationId)
    {
        var res = await _reservationRepository.GetReservationWithDetailsAsync(reservationId);
        if (res == null)
            throw new KeyNotFoundException("Reservation not found.");

        res.ReservationStatus = ReservationStatus.Cancelled;
        await _reservationRepository.UpdateAsync(res);

        if (res.Room != null && res.Room.Status == RoomStatus.Booked)
        {
            res.Room.Status = RoomStatus.Available;
            await _roomRepository.UpdateAsync(res.Room);
        }
    }

    public async Task DeleteReservationAsync(int id)
    {
        await _reservationRepository.DeleteAsync(id);
    }

    private static ReservationResponseDTO MapToDTO(Reservation res) => new()
    {
        ReservationId = res.ReservationId,
        GuestId = res.GuestId,
        GuestName = res.Guest?.FullName ?? "Unknown",
        RoomId = res.RoomId,
        RoomNumber = res.Room?.RoomNumber ?? "N/A",
        RoomType = res.Room?.RoomType ?? "N/A",
        PricePerNight = res.Room?.PricePerNight ?? 0,
        CheckInDate = res.CheckInDate,
        CheckOutDate = res.CheckOutDate,
        ReservationStatus = res.ReservationStatus
    };
}

public class CheckInService : ICheckInService
{
    private readonly IStayRecordRepository _stayRecordRepository;
    private readonly IReservationRepository _reservationRepository;
    private readonly IRoomRepository _roomRepository;

    public CheckInService(
        IStayRecordRepository stayRecordRepository,
        IReservationRepository reservationRepository,
        IRoomRepository roomRepository)
    {
        _stayRecordRepository = stayRecordRepository;
        _reservationRepository = reservationRepository;
        _roomRepository = roomRepository;
    }

    public async Task<IEnumerable<StayRecordResponseDTO>> GetAllStaysAsync()
    {
        var stays = await _stayRecordRepository.GetStayRecordsWithDetailsAsync();
        return stays.Select(MapToDTO);
    }

    public async Task<StayRecordResponseDTO?> GetStayByIdAsync(int id)
    {
        var stay = await _stayRecordRepository.GetStayRecordWithDetailsAsync(id);
        return stay != null ? MapToDTO(stay) : null;
    }

    public async Task<StayRecordResponseDTO?> GetStayByReservationIdAsync(int reservationId)
    {
        var stay = await _stayRecordRepository.GetStayRecordByReservationAsync(reservationId);
        return stay != null ? MapToDTO(stay) : null;
    }

    public async Task<StayRecordResponseDTO> CheckInGuestAsync(CreateCheckInDTO dto)
    {
        var reservation = await _reservationRepository.GetReservationWithDetailsAsync(dto.ReservationId);
        if (reservation == null)
            throw new InvalidOperationException("Reservation not found.");

        if (reservation.ReservationStatus != ReservationStatus.Booked)
        {
            throw new InvalidOperationException("Only confirmed reservations can be checked in.");
        }

        var existingStay = await _stayRecordRepository.GetStayRecordByReservationAsync(dto.ReservationId);
        if (existingStay != null)
        {
            throw new InvalidOperationException("Guest is already checked in for this reservation.");
        }

        var stay = new StayRecord
        {
            GuestId = reservation.GuestId,
            ReservationId = reservation.ReservationId,
            ActualCheckIn = DateTime.Now
        };

        await _stayRecordRepository.AddAsync(stay);

        // Update Room status to Occupied
        if (reservation.Room != null)
        {
            reservation.Room.Status = RoomStatus.Occupied;
            await _roomRepository.UpdateAsync(reservation.Room);
        }

        var result = await _stayRecordRepository.GetStayRecordWithDetailsAsync(stay.StayId);
        return MapToDTO(result ?? stay);
    }

    public async Task RequestCheckOutAsync(int stayId)
    {
        var stay = await _stayRecordRepository.GetStayRecordWithDetailsAsync(stayId);
        if (stay == null)
            throw new KeyNotFoundException("Stay record not found.");

        if (stay.Reservation?.Room != null)
        {
            stay.Reservation.Room.Status = RoomStatus.CheckoutPending;
            await _roomRepository.UpdateAsync(stay.Reservation.Room);
        }
    }

    public async Task CompleteCheckOutAsync(int stayId)
    {
        var stay = await _stayRecordRepository.GetStayRecordWithDetailsAsync(stayId);
        if (stay == null)
            throw new KeyNotFoundException("Stay record not found.");

        stay.ActualCheckOut = DateTime.Now;
        await _stayRecordRepository.UpdateAsync(stay);

        if (stay.Reservation?.Room != null)
        {
            stay.Reservation.Room.Status = RoomStatus.CleaningRequired;
            await _roomRepository.UpdateAsync(stay.Reservation.Room);
        }
    }

    public async Task DeleteStayAsync(int id)
    {
        await _stayRecordRepository.DeleteAsync(id);
    }

    private static StayRecordResponseDTO MapToDTO(StayRecord stay) => new()
    {
        StayId = stay.StayId,
        GuestId = stay.GuestId,
        GuestName = stay.Guest?.FullName ?? "Unknown",
        ReservationId = stay.ReservationId,
        RoomNumber = stay.Reservation?.Room?.RoomNumber ?? "N/A",
        ActualCheckIn = stay.ActualCheckIn,
        ActualCheckOut = stay.ActualCheckOut,
        Billing = stay.Billing != null ? new BillingResponseDTO
        {
            BillId = stay.Billing.BillId,
            StayId = stay.Billing.StayId,
            TotalAmount = stay.Billing.TotalAmount,
            PaymentStatus = stay.Billing.PaymentStatus,
            Remarks = stay.Billing.Remarks
        } : null
    };
}

public class BillingService : IBillingService
{
    private readonly IBillingRepository _billingRepository;
    private readonly IStayRecordRepository _stayRecordRepository;
    private readonly IRoomRepository _roomRepository;
    private readonly IHousekeepingTaskRepository _housekeepingTaskRepository;

    public BillingService(
        IBillingRepository billingRepository,
        IStayRecordRepository stayRecordRepository,
        IRoomRepository roomRepository,
        IHousekeepingTaskRepository housekeepingTaskRepository)
    {
        _billingRepository = billingRepository;
        _stayRecordRepository = stayRecordRepository;
        _roomRepository = roomRepository;
        _housekeepingTaskRepository = housekeepingTaskRepository;
    }

    public async Task<IEnumerable<BillingResponseDTO>> GetAllBillsAsync()
    {
        var bills = await _billingRepository.GetBillingsWithDetailsAsync();
        return bills.Select(MapToDTO);
    }

    public async Task<BillingResponseDTO?> GetBillByIdAsync(int id)
    {
        var bill = await _billingRepository.GetBillingWithDetailsAsync(id);
        return bill != null ? MapToDTO(bill) : null;
    }

    public async Task<BillingResponseDTO?> GetBillByStayIdAsync(int stayId)
    {
        var bill = await _billingRepository.GetBillingByStayIdAsync(stayId);
        return bill != null ? MapToDTO(bill) : null;
    }

    public async Task<BillingResponseDTO> GenerateBillForStayAsync(int stayId, string? remarks = null)
    {
        var stay = await _stayRecordRepository.GetStayRecordWithDetailsAsync(stayId);
        if (stay == null)
            throw new InvalidOperationException("Stay record not found.");

        var existingBill = await _billingRepository.GetBillingByStayIdAsync(stayId);
        if (existingBill != null)
        {
            return MapToDTO(existingBill);
        }

        var res = stay.Reservation;
        int nights = (res.CheckOutDate - res.CheckInDate).Days;
        if (nights <= 0) nights = 1;

        decimal price = res.Room?.PricePerNight ?? 100;
        decimal totalAmount = price * nights;

        var bill = new Billing
        {
            StayId = stayId,
            TotalAmount = totalAmount,
            PaymentStatus = PaymentStatus.Pending,
            Remarks = remarks ?? $"Room charge for {nights} night(s) @ {price:C}/night"
        };

        await _billingRepository.AddAsync(bill);
        var created = await _billingRepository.GetBillingWithDetailsAsync(bill.BillId);
        return MapToDTO(created ?? bill);
    }

    public async Task<BillingResponseDTO> CreateBillAsync(CreateBillDTO dto)
    {
        var bill = new Billing
        {
            StayId = dto.StayId,
            TotalAmount = dto.TotalAmount,
            PaymentStatus = PaymentStatus.Pending,
            Remarks = dto.Remarks
        };

        await _billingRepository.AddAsync(bill);
        var created = await _billingRepository.GetBillingWithDetailsAsync(bill.BillId);
        return MapToDTO(created ?? bill);
    }

    public async Task ProcessPaymentAsync(ProcessPaymentDTO dto)
    {
        var bill = await _billingRepository.GetBillingWithDetailsAsync(dto.BillId);
        if (bill == null)
            throw new KeyNotFoundException("Bill record not found.");

        bill.PaymentStatus = PaymentStatus.Paid;
        bill.Remarks = $"{bill.Remarks} | Paid: {dto.Remarks}";
        await _billingRepository.UpdateAsync(bill);

        // Update stay check-out time & room workflow status
        var stay = bill.StayRecord;
        if (stay != null)
        {
            stay.ActualCheckOut = DateTime.Now;
            await _stayRecordRepository.UpdateAsync(stay);

            if (stay.Reservation?.Room != null)
            {
                var room = stay.Reservation.Room;
                // Transition room status to CleaningRequired
                room.Status = RoomStatus.CleaningRequired;
                await _roomRepository.UpdateAsync(room);

                // AUTOMATICALLY CREATE HOUSEKEEPING CLEANING TASK FOR HOUSEKEEPING MODULE
                var cleaningTask = new HousekeepingTask
                {
                    RoomId = room.RoomId,
                    TaskDescription = $"Room Cleaning Request following Guest Check-Out (Bill #{bill.BillId})",
                    TaskStatus = TaskStatus.Pending
                };
                await _housekeepingTaskRepository.AddAsync(cleaningTask);
            }
        }
    }

    public async Task DeleteBillAsync(int id)
    {
        await _billingRepository.DeleteAsync(id);
    }

    private static BillingResponseDTO MapToDTO(Billing bill) => new()
    {
        BillId = bill.BillId,
        StayId = bill.StayId,
        GuestName = bill.StayRecord?.Guest?.FullName ?? "Unknown",
        RoomNumber = bill.StayRecord?.Reservation?.Room?.RoomNumber ?? "N/A",
        TotalAmount = bill.TotalAmount,
        PaymentStatus = bill.PaymentStatus,
        Remarks = bill.Remarks
    };
}

public class HousekeepingService : IHousekeepingService
{
    private readonly IHousekeepingTaskRepository _taskRepository;
    private readonly IRoomRepository _roomRepository;

    public HousekeepingService(
        IHousekeepingTaskRepository taskRepository,
        IRoomRepository roomRepository)
    {
        _taskRepository = taskRepository;
        _roomRepository = roomRepository;
    }

    public async Task<IEnumerable<HousekeepingTaskResponseDTO>> GetAllTasksAsync()
    {
        var tasks = await _taskRepository.GetTasksWithDetailsAsync();
        return tasks.Select(MapToDTO);
    }

    public async Task<HousekeepingTaskResponseDTO?> GetTaskByIdAsync(int id)
    {
        var task = await _taskRepository.GetTaskWithDetailsAsync(id);
        return task != null ? MapToDTO(task) : null;
    }

    public async Task<IEnumerable<HousekeepingTaskResponseDTO>> GetTasksByRoomIdAsync(int roomId)
    {
        var tasks = await _taskRepository.GetTasksByRoomIdAsync(roomId);
        return tasks.Select(MapToDTO);
    }

    public async Task<HousekeepingTaskResponseDTO> CreateTaskAsync(CreateHousekeepingTaskDTO dto)
    {
        var room = await _roomRepository.GetByIdAsync(dto.RoomId);
        if (room == null)
            throw new InvalidOperationException("Room not found.");

        var task = new HousekeepingTask
        {
            RoomId = dto.RoomId,
            TaskDescription = dto.TaskDescription,
            TaskStatus = TaskStatus.Pending
        };

        await _taskRepository.AddAsync(task);
        var created = await _taskRepository.GetTaskWithDetailsAsync(task.TaskId);
        return MapToDTO(created ?? task);
    }

    public async Task UpdateTaskStatusAsync(UpdateTaskStatusDTO dto)
    {
        var task = await _taskRepository.GetTaskWithDetailsAsync(dto.TaskId);
        if (task == null)
            throw new KeyNotFoundException("Housekeeping task not found.");

        task.TaskStatus = dto.TaskStatus;
        await _taskRepository.UpdateAsync(task);

        // Room status state machine update
        if (task.Room != null)
        {
            if (dto.TaskStatus == TaskStatus.InProgress)
            {
                task.Room.Status = RoomStatus.CleaningInProgress;
                await _roomRepository.UpdateAsync(task.Room);
            }
            else if (dto.TaskStatus == TaskStatus.Completed)
            {
                task.Room.Status = RoomStatus.Available; // Visible again for public booking!
                await _roomRepository.UpdateAsync(task.Room);
            }
        }
    }

    public async Task DeleteTaskAsync(int id)
    {
        await _taskRepository.DeleteAsync(id);
    }

    private static HousekeepingTaskResponseDTO MapToDTO(HousekeepingTask task) => new()
    {
        TaskId = task.TaskId,
        RoomId = task.RoomId,
        RoomNumber = task.Room?.RoomNumber ?? "N/A",
        TaskDescription = task.TaskDescription,
        TaskStatus = task.TaskStatus
    };
}

public class StaffService : IStaffService
{
    private readonly IStaffRepository _staffRepository;

    public StaffService(IStaffRepository staffRepository)
    {
        _staffRepository = staffRepository;
    }

    public async Task<IEnumerable<StaffResponseDTO>> GetAllStaffAsync()
    {
        var staffList = await _staffRepository.GetAllAsync();
        return staffList.Select(MapToDTO);
    }

    public async Task<StaffResponseDTO?> GetStaffByIdAsync(int id)
    {
        var staff = await _staffRepository.GetByIdAsync(id);
        return staff != null ? MapToDTO(staff) : null;
    }

    public async Task<StaffResponseDTO> CreateStaffAsync(CreateStaffDTO dto)
    {
        var existing = await _staffRepository.GetByEmailAsync(dto.Email);
        if (existing != null)
            throw new InvalidOperationException("Staff member with this email already exists.");

        var staff = new Staff
        {
            FullName = dto.FullName,
            Email = dto.Email,
            PhoneNumber = dto.PhoneNumber,
            PasswordHash = HashPassword(dto.Password),
            Role = dto.Role,
            IsActive = true,
            CreatedAt = DateTime.Now
        };

        await _staffRepository.AddAsync(staff);
        return MapToDTO(staff);
    }

    public async Task UpdateStaffAsync(UpdateStaffDTO dto)
    {
        var staff = await _staffRepository.GetByIdAsync(dto.StaffId);
        if (staff == null)
            throw new KeyNotFoundException("Staff member not found.");

        staff.FullName = dto.FullName;
        staff.Email = dto.Email;
        staff.PhoneNumber = dto.PhoneNumber;
        staff.Role = dto.Role;
        staff.IsActive = dto.IsActive;

        await _staffRepository.UpdateAsync(staff);
    }

    public async Task DeleteStaffAsync(int id)
    {
        await _staffRepository.DeleteAsync(id);
    }

    public async Task<StaffResponseDTO?> ValidateStaffLoginAsync(StaffLoginDTO dto)
    {
        var staff = await _staffRepository.GetByEmailAsync(dto.Email);
        if (staff == null || !staff.IsActive || staff.PasswordHash != HashPassword(dto.Password))
        {
            return null;
        }

        if (staff.Role != dto.Role && dto.Role != StaffRole.Admin)
        {
            return null; // Role mismatch
        }

        return MapToDTO(staff);
    }

    private static StaffResponseDTO MapToDTO(Staff staff) => new()
    {
        StaffId = staff.StaffId,
        FullName = staff.FullName,
        Email = staff.Email,
        PhoneNumber = staff.PhoneNumber,
        Role = staff.Role,
        IsActive = staff.IsActive,
        CreatedAt = staff.CreatedAt
    };

    private static string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(bytes);
    }
}

public class FeedbackService : IFeedbackService
{
    private readonly IFeedbackRepository _feedbackRepository;
    private readonly IGuestRepository _guestRepository;

    public FeedbackService(
        IFeedbackRepository feedbackRepository,
        IGuestRepository guestRepository)
    {
        _feedbackRepository = feedbackRepository;
        _guestRepository = guestRepository;
    }

    public async Task<IEnumerable<FeedbackResponseDTO>> GetAllFeedbacksAsync()
    {
        var feedbacks = await _feedbackRepository.GetFeedbacksWithDetailsAsync();
        return feedbacks.Select(MapToDTO);
    }

    public async Task<FeedbackResponseDTO?> GetFeedbackByIdAsync(int id)
    {
        var feedbacks = await _feedbackRepository.GetFeedbacksWithDetailsAsync();
        var feedback = feedbacks.FirstOrDefault(f => f.FeedbackId == id);
        return feedback != null ? MapToDTO(feedback) : null;
    }

    public async Task<FeedbackResponseDTO> SubmitFeedbackAsync(CreateFeedbackDTO dto)
    {
        var guest = await _guestRepository.GetByIdAsync(dto.GuestId);
        if (guest == null)
            throw new InvalidOperationException("Guest account not found.");

        var feedback = new Feedback
        {
            GuestId = dto.GuestId,
            ReservationId = dto.ReservationId,
            Rating = dto.Rating,
            Comments = dto.Comments,
            CreatedAt = DateTime.Now
        };

        await _feedbackRepository.AddAsync(feedback);
        return MapToDTO(feedback);
    }

    public async Task DeleteFeedbackAsync(int id)
    {
        await _feedbackRepository.DeleteAsync(id);
    }

    private static FeedbackResponseDTO MapToDTO(Feedback f) => new()
    {
        FeedbackId = f.FeedbackId,
        GuestId = f.GuestId,
        GuestName = f.Guest?.FullName ?? "Guest",
        ReservationId = f.ReservationId,
        Rating = f.Rating,
        Comments = f.Comments,
        CreatedAt = f.CreatedAt
    };
}
