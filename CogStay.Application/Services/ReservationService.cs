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

public class ReservationService : IReservationService
{
    private readonly IReservationRepository _reservationRepository;
    private readonly IGuestRepository _guestRepository;
    private readonly IRoomRepository _roomRepository;

    public ReservationService(
        IReservationRepository reservationRepository,
        IGuestRepository guestRepository,
        IRoomRepository roomRepository)
    {
        _reservationRepository = reservationRepository;
        _guestRepository = guestRepository;
        _roomRepository = roomRepository;
    }

    public async Task<IEnumerable<ReservationResponseDTO>> GetAllReservationsAsync()
    {
        var reservations = await _reservationRepository.GetAllAsync();
        return await MapToDTOListAsync(reservations);
    }

    public async Task<ReservationResponseDTO?> GetReservationByIdAsync(int id)
    {
        var reservation = await _reservationRepository.GetByIdAsync(id);
        if (reservation == null) return null;
        var guest = await _guestRepository.GetByIdAsync(reservation.GuestId);
        var room = await _roomRepository.GetByIdAsync(reservation.RoomId);
        return MapToDTO(reservation, guest, room);
    }

    public async Task<IEnumerable<ReservationResponseDTO>> GetReservationsByGuestAsync(int guestId)
    {
        var reservations = await _reservationRepository.GetByGuestIdAsync(guestId);
        return await MapToDTOListAsync(reservations);
    }

    public async Task<ReservationResponseDTO> BookRoomAsync(CreateReservationDTO dto)
    {
        if (dto.CheckInDate >= dto.CheckOutDate)
        {
            throw new ArgumentException("Check-in date must be earlier than Check-out date.");
        }

        var guest = await _guestRepository.GetByIdAsync(dto.GuestId);
        if (guest == null)
        {
            throw new KeyNotFoundException($"Guest with ID {dto.GuestId} not found.");
        }

        var room = await _roomRepository.GetByIdAsync(dto.RoomId);
        if (room == null)
        {
            throw new KeyNotFoundException($"Room with ID {dto.RoomId} not found.");
        }

        // Concurrency / Availability Check: Prevent double booking
        bool hasConflict = await _reservationRepository.HasConflictingReservationAsync(dto.RoomId, dto.CheckInDate, dto.CheckOutDate);
        if (hasConflict)
        {
            throw new InvalidOperationException($"Room {room.RoomNumber} is already reserved for the selected date range.");
        }

        var nextId = await _reservationRepository.GetNextReservationIdAsync();
        var reservation = new Reservation
        {
            ReservationId = nextId,
            GuestId = dto.GuestId,
            GuestName = guest.FullName,
            RoomId = dto.RoomId,
            CheckInDate = dto.CheckInDate,
            CheckOutDate = dto.CheckOutDate,
            ReservationStatus = ReservationStatus.Confirmed,
            Version = 1
        };

        await _reservationRepository.CreateAsync(reservation);

        // Update room status to Reserved if checking in today
        if (dto.CheckInDate.Date == DateTime.Today)
        {
            room.Status = RoomStatus.Reserved;
            await _roomRepository.UpdateAsync(room);
        }

        return MapToDTO(reservation, guest, room);
    }

    public async Task UpdateReservationAsync(UpdateReservationDTO dto)
    {
        var reservation = await _reservationRepository.GetByIdAsync(dto.ReservationId);
        if (reservation == null)
        {
            throw new KeyNotFoundException($"Reservation with ID {dto.ReservationId} not found.");
        }

        bool hasConflict = await _reservationRepository.HasConflictingReservationAsync(
            dto.RoomId, dto.CheckInDate, dto.CheckOutDate, dto.ReservationId);
        if (hasConflict)
        {
            throw new InvalidOperationException($"Room is already reserved for the selected dates.");
        }

        var guest = await _guestRepository.GetByIdAsync(dto.GuestId);
        if (guest != null) reservation.GuestName = guest.FullName;

        reservation.GuestId = dto.GuestId;
        reservation.RoomId = dto.RoomId;
        reservation.CheckInDate = dto.CheckInDate;
        reservation.CheckOutDate = dto.CheckOutDate;
        reservation.ReservationStatus = dto.ReservationStatus;
        reservation.Version++;

        await _reservationRepository.UpdateAsync(reservation);
    }

    public async Task CancelReservationAsync(int reservationId)
    {
        var reservation = await _reservationRepository.GetByIdAsync(reservationId);
        if (reservation == null)
        {
            throw new KeyNotFoundException($"Reservation with ID {reservationId} not found.");
        }

        reservation.ReservationStatus = ReservationStatus.Cancelled;
        await _reservationRepository.UpdateAsync(reservation);

        // Reset room status if needed
        var room = await _roomRepository.GetByIdAsync(reservation.RoomId);
        if (room != null && room.Status == RoomStatus.Reserved)
        {
            room.Status = RoomStatus.Available;
            await _roomRepository.UpdateAsync(room);
        }
    }

    public async Task DeleteReservationAsync(int id)
    {
        await _reservationRepository.DeleteAsync(id);
    }

    private async Task<IEnumerable<ReservationResponseDTO>> MapToDTOListAsync(IEnumerable<Reservation> list)
    {
        var dtos = new List<ReservationResponseDTO>();
        foreach (var r in list)
        {
            var guest = await _guestRepository.GetByIdAsync(r.GuestId);
            var room = await _roomRepository.GetByIdAsync(r.RoomId);
            dtos.Add(MapToDTO(r, guest, room));
        }
        return dtos;
    }

    private static ReservationResponseDTO MapToDTO(Reservation r, Guest? g, Room? rm) => new()
    {
        ReservationId = r.ReservationId,
        GuestId = r.GuestId,
        GuestName = g?.FullName ?? r.GuestName,
        RoomId = r.RoomId,
        RoomNumber = rm?.RoomNumber ?? "N/A",
        RoomType = rm?.RoomType ?? "N/A",
        PricePerNight = rm?.PricePerNight ?? 0,
        CheckInDate = r.CheckInDate,
        CheckOutDate = r.CheckOutDate,
        ReservationStatus = r.ReservationStatus
    };
}
