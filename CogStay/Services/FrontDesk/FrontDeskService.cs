using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CogStayMVC.DTOs;
using CogStayMVC.Enums;
using CogStayMVC.Models;
using CogStayMVC.Repositories.Interfaces;
using CogStayMVC.Services.Interfaces;
using TaskStatus = CogStayMVC.Enums.TaskStatus;

namespace CogStayMVC.Services.FrontDesk;

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
            GuestName = guest.FullName,
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
        GuestName = string.IsNullOrEmpty(res.GuestName) ? (res.Guest?.FullName ?? "Unknown") : res.GuestName,
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
            ActualCheckIn = DateTime.Now,
            GuestName = string.IsNullOrEmpty(reservation.GuestName) ? (reservation.Guest?.FullName ?? "Unknown") : reservation.GuestName,
            BookingReference = $"BK-{reservation.ReservationId}",
            BillingReference = "Pending",
            StayDetails = $"Room {reservation.Room?.RoomNumber ?? "N/A"} - Stay from {reservation.CheckInDate:yyyy-MM-dd} to {reservation.CheckOutDate:yyyy-MM-dd}"
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

        if (stay.Reservation != null)
        {
            stay.Reservation.ReservationStatus = ReservationStatus.CheckedOut;
            await _reservationRepository.UpdateAsync(stay.Reservation);
        }

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
        GuestName = string.IsNullOrEmpty(stay.GuestName) ? (stay.Guest?.FullName ?? "Unknown") : stay.GuestName,
        ReservationId = stay.ReservationId,
        RoomNumber = stay.Reservation?.Room?.RoomNumber ?? "N/A",
        ActualCheckIn = stay.ActualCheckIn,
        ActualCheckOut = stay.ActualCheckOut,
        BookingReference = stay.BookingReference,
        BillingReference = stay.BillingReference,
        StayDetails = stay.StayDetails,
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
    private readonly IReservationRepository _reservationRepository;

    public BillingService(
        IBillingRepository billingRepository,
        IStayRecordRepository stayRecordRepository,
        IRoomRepository roomRepository,
        IHousekeepingTaskRepository housekeepingTaskRepository,
        IReservationRepository reservationRepository)
    {
        _billingRepository = billingRepository;
        _stayRecordRepository = stayRecordRepository;
        _roomRepository = roomRepository;
        _housekeepingTaskRepository = housekeepingTaskRepository;
        _reservationRepository = reservationRepository;
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
            GuestId = stay.GuestId,
            GuestName = string.IsNullOrEmpty(stay.GuestName) ? (stay.Guest?.FullName ?? "Unknown") : stay.GuestName,
            TotalAmount = totalAmount,
            PaymentStatus = PaymentStatus.Pending,
            Remarks = remarks ?? $"Room charge for {nights} night(s) @ {price:C}/night"
        };

        await _billingRepository.AddAsync(bill);

        if (stay != null)
        {
            stay.BillingReference = $"BILL-{bill.BillId}";
            await _stayRecordRepository.UpdateAsync(stay);
        }

        var created = await _billingRepository.GetBillingWithDetailsAsync(bill.BillId);
        return MapToDTO(created ?? bill);
    }

    public async Task<BillingResponseDTO> CreateBillAsync(CreateBillDTO dto)
    {
        var stay = await _stayRecordRepository.GetStayRecordWithDetailsAsync(dto.StayId);
        if (stay == null)
            throw new InvalidOperationException("Stay record not found.");

        var bill = new Billing
        {
            StayId = dto.StayId,
            GuestId = stay.GuestId,
            GuestName = string.IsNullOrEmpty(stay.GuestName) ? (stay.Guest?.FullName ?? "Unknown") : stay.GuestName,
            TotalAmount = dto.TotalAmount,
            PaymentStatus = PaymentStatus.Pending,
            Remarks = dto.Remarks
        };

        await _billingRepository.AddAsync(bill);

        stay.BillingReference = $"BILL-{bill.BillId}";
        await _stayRecordRepository.UpdateAsync(stay);

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

            if (stay.Reservation != null)
            {
                stay.Reservation.ReservationStatus = ReservationStatus.CheckedOut;
                await _reservationRepository.UpdateAsync(stay.Reservation);
            }

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
        GuestName = string.IsNullOrEmpty(bill.GuestName) ? (bill.StayRecord?.Guest?.FullName ?? "Unknown") : bill.GuestName,
        RoomNumber = bill.StayRecord?.Reservation?.Room?.RoomNumber ?? "N/A",
        TotalAmount = bill.TotalAmount,
        PaymentStatus = bill.PaymentStatus,
        Remarks = bill.Remarks
    };
}
