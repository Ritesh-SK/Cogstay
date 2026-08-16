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
        var rooms = await _roomRepository.GetAvailableRoomsAsync();
        return rooms.Select(MapToDTO);
    }

    public async Task<RoomResponseDTO?> GetRoomByIdAsync(int id)
    {
        var room = await _roomRepository.GetByIdAsync(id);
        return room == null ? null : MapToDTO(room);
    }

    public async Task<RoomResponseDTO> CreateRoomAsync(CreateRoomDTO dto)
    {
        var existing = await _roomRepository.GetByRoomNumberAsync(dto.RoomNumber);
        if (existing != null)
        {
            throw new InvalidOperationException($"Room with number '{dto.RoomNumber}' already exists.");
        }

        var nextId = await _roomRepository.GetNextRoomIdAsync();
        var room = new Room
        {
            RoomId = nextId,
            RoomNumber = dto.RoomNumber,
            RoomType = dto.RoomType,
            PricePerNight = dto.PricePerNight,
            Status = dto.Status
        };

        await _roomRepository.CreateAsync(room);
        return MapToDTO(room);
    }

    public async Task UpdateRoomAsync(UpdateRoomDTO dto)
    {
        var room = await _roomRepository.GetByIdAsync(dto.RoomId);
        if (room == null)
        {
            throw new KeyNotFoundException($"Room with ID {dto.RoomId} not found.");
        }

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
        {
            throw new KeyNotFoundException($"Room with ID {roomId} not found.");
        }

        room.Status = status;
        await _roomRepository.UpdateAsync(room);
    }

    public async Task DeleteRoomAsync(int id)
    {
        await _roomRepository.DeleteAsync(id);
    }

    private static RoomResponseDTO MapToDTO(Room r) => new()
    {
        RoomId = r.RoomId,
        RoomNumber = r.RoomNumber,
        RoomType = r.RoomType,
        PricePerNight = r.PricePerNight,
        Status = r.Status
    };
}
