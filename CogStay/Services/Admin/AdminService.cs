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

namespace CogStayMVC.Services.Admin;

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
