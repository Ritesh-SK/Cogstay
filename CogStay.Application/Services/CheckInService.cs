using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CogStay.Application.Contracts.Persistence;
using CogStay.Application.Contracts.Services;
using CogStay.Application.DTOs;
using CogStay.Domain.Entities;
using CogStay.Domain.Enums;

namespace CogStay.Application.Services;

public class CheckInService : ICheckInService
{
    private readonly IStayRecordRepository _stayRepository;
    private readonly IReservationRepository _reservationRepository;
    private readonly IGuestRepository _guestRepository;
    private readonly IRoomRepository _roomRepository;
    private readonly IBillingRepository _billingRepository;

    public CheckInService(
        IStayRecordRepository stayRepository,
        IReservationRepository reservationRepository,
        IGuestRepository guestRepository,
        IRoomRepository roomRepository,
        IBillingRepository billingRepository)
    {
        _stayRepository = stayRepository;
        _reservationRepository = reservationRepository;
        _guestRepository = guestRepository;
        _roomRepository = roomRepository;
        _billingRepository = billingRepository;
    }

    public async Task<IEnumerable<StayRecordResponseDTO>> GetAllStaysAsync()
    {
        var stays = await _stayRepository.GetAllAsync();
        return await MapToDTOListAsync(stays);
    }

    public async Task<StayRecordResponseDTO?> GetStayByIdAsync(int id)
    {
        var stay = await _stayRepository.GetByIdAsync(id);
        if (stay == null) return null;
        return await MapToDTOAsync(stay);
    }

    public async Task<StayRecordResponseDTO?> GetStayByReservationIdAsync(int reservationId)
    {
        var stay = await _stayRepository.GetByReservationIdAsync(reservationId);
        if (stay == null) return null;
        return await MapToDTOAsync(stay);
    }

    public async Task<StayRecordResponseDTO> CheckInGuestAsync(CreateCheckInDTO dto)
    {
        var reservation = await _reservationRepository.GetByIdAsync(dto.ReservationId);
        if (reservation == null)
        {
            throw new KeyNotFoundException($"Reservation with ID {dto.ReservationId} not found.");
        }

        var existingStay = await _stayRepository.GetByReservationIdAsync(dto.ReservationId);
        if (existingStay != null)
        {
            throw new InvalidOperationException($"Guest is already checked in for Reservation ID {dto.ReservationId}.");
        }

        var guest = await _guestRepository.GetByIdAsync(reservation.GuestId);
        var room = await _roomRepository.GetByIdAsync(reservation.RoomId);

        var nextId = await _stayRepository.GetNextStayIdAsync();
        var stay = new StayRecord
        {
            StayId = nextId,
            GuestId = reservation.GuestId,
            ReservationId = reservation.ReservationId,
            ActualCheckIn = DateTime.UtcNow,
            GuestName = guest?.FullName ?? reservation.GuestName,
            BookingReference = $"REF-RES-{reservation.ReservationId}",
            StayDetails = $"Checked in room {room?.RoomNumber}"
        };

        await _stayRepository.CreateAsync(stay);

        // Update reservation and room status
        reservation.ReservationStatus = ReservationStatus.CheckedIn;
        await _reservationRepository.UpdateAsync(reservation);

        if (room != null)
        {
            room.Status = RoomStatus.Occupied;
            await _roomRepository.UpdateAsync(room);
        }

        return await MapToDTOAsync(stay);
    }

    public async Task RequestCheckOutAsync(int stayId)
    {
        var stay = await _stayRepository.GetByIdAsync(stayId);
        if (stay == null)
        {
            throw new KeyNotFoundException($"Stay record with ID {stayId} not found.");
        }

        stay.StayDetails = (stay.StayDetails ?? "") + " | CheckOut Requested";
        await _stayRepository.UpdateAsync(stay);
    }

    public async Task CompleteCheckOutAsync(int stayId)
    {
        var stay = await _stayRepository.GetByIdAsync(stayId);
        if (stay == null)
        {
            throw new KeyNotFoundException($"Stay record with ID {stayId} not found.");
        }

        stay.ActualCheckOut = DateTime.UtcNow;
        await _stayRepository.UpdateAsync(stay);

        var reservation = await _reservationRepository.GetByIdAsync(stay.ReservationId);
        if (reservation != null)
        {
            reservation.ReservationStatus = ReservationStatus.CheckedOut;
            await _reservationRepository.UpdateAsync(reservation);

            var room = await _roomRepository.GetByIdAsync(reservation.RoomId);
            if (room != null)
            {
                room.Status = RoomStatus.NeedsCleaning;
                await _roomRepository.UpdateAsync(room);
            }
        }
    }

    public async Task DeleteStayAsync(int id)
    {
        await _stayRepository.DeleteAsync(id);
    }

    private async Task<IEnumerable<StayRecordResponseDTO>> MapToDTOListAsync(IEnumerable<StayRecord> list)
    {
        var dtos = new List<StayRecordResponseDTO>();
        foreach (var s in list)
        {
            dtos.Add(await MapToDTOAsync(s));
        }
        return dtos;
    }

    private async Task<StayRecordResponseDTO> MapToDTOAsync(StayRecord s)
    {
        var reservation = await _reservationRepository.GetByIdAsync(s.ReservationId);
        var room = reservation != null ? await _roomRepository.GetByIdAsync(reservation.RoomId) : null;
        var billing = await _billingRepository.GetByStayIdAsync(s.StayId);

        return new StayRecordResponseDTO
        {
            StayId = s.StayId,
            GuestId = s.GuestId,
            GuestName = s.GuestName,
            ReservationId = s.ReservationId,
            RoomNumber = room?.RoomNumber ?? "N/A",
            ActualCheckIn = s.ActualCheckIn,
            ActualCheckOut = s.ActualCheckOut,
            BookingReference = s.BookingReference,
            BillingReference = s.BillingReference,
            StayDetails = s.StayDetails,
            Billing = billing == null ? null : new BillingResponseDTO
            {
                BillId = billing.BillId,
                StayId = billing.StayId,
                GuestName = billing.GuestName,
                RoomNumber = room?.RoomNumber ?? "N/A",
                TotalAmount = billing.TotalAmount,
                PaymentStatus = billing.PaymentStatus,
                Remarks = billing.Remarks
            }
        };
    }
}
