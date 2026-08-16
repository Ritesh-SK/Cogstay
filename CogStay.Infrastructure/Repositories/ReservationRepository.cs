using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Driver;
using CogStay.Application.Contracts.Persistence;
using CogStay.Domain.Entities;
using CogStay.Domain.Enums;
using CogStay.Infrastructure.Data;

namespace CogStay.Infrastructure.Repositories;

public class ReservationRepository : IReservationRepository
{
    private readonly MongoDbContext _context;

    public ReservationRepository(MongoDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Reservation>> GetAllAsync()
    {
        return await _context.Reservations.Find(_ => true).ToListAsync();
    }

    public async Task<Reservation?> GetByIdAsync(int id)
    {
        return await _context.Reservations.Find(r => r.ReservationId == id).FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<Reservation>> GetByGuestIdAsync(int guestId)
    {
        return await _context.Reservations.Find(r => r.GuestId == guestId).ToListAsync();
    }

    public async Task<Reservation> CreateAsync(Reservation reservation)
    {
        await _context.Reservations.InsertOneAsync(reservation);
        return reservation;
    }

    public async Task UpdateAsync(Reservation reservation)
    {
        await _context.Reservations.ReplaceOneAsync(r => r.Id == reservation.Id, reservation);
    }

    public async Task DeleteAsync(int id)
    {
        await _context.Reservations.DeleteOneAsync(r => r.ReservationId == id);
    }

    public async Task<int> GetNextReservationIdAsync()
    {
        var maxRes = await _context.Reservations
            .Find(_ => true)
            .SortByDescending(r => r.ReservationId)
            .FirstOrDefaultAsync();

        return maxRes == null ? 1 : maxRes.ReservationId + 1;
    }

    public async Task<bool> HasConflictingReservationAsync(int roomId, DateTime checkIn, DateTime checkOut, int? excludeReservationId = null)
    {
        var filter = Builders<Reservation>.Filter.And(
            Builders<Reservation>.Filter.Eq(r => r.RoomId, roomId),
            Builders<Reservation>.Filter.Ne(r => r.ReservationStatus, ReservationStatus.Cancelled),
            Builders<Reservation>.Filter.Lt(r => r.CheckInDate, checkOut),
            Builders<Reservation>.Filter.Gt(r => r.CheckOutDate, checkIn)
        );

        if (excludeReservationId.HasValue)
        {
            filter = Builders<Reservation>.Filter.And(
                filter,
                Builders<Reservation>.Filter.Ne(r => r.ReservationId, excludeReservationId.Value)
            );
        }

        long count = await _context.Reservations.CountDocumentsAsync(filter);
        return count > 0;
    }
}
