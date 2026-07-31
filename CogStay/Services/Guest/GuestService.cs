using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using CogStayMVC.DTOs;
using CogStayMVC.Models;
using CogStayMVC.Repositories.Interfaces;
using CogStayMVC.Services.Interfaces;

namespace CogStayMVC.Services.GuestModule;

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
