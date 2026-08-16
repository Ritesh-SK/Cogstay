using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using CogStay.Application.Contracts.Persistence;
using CogStay.Application.Contracts.Services;
using CogStay.Application.DTOs;
using CogStay.Domain.Entities;

namespace CogStay.Application.Services;

public class GuestService : IGuestService
{
    private readonly IGuestRepository _guestRepository;
    private readonly IPasswordHasher<Guest> _passwordHasher;
    private readonly IOtpService _otpService;

    public GuestService(
        IGuestRepository guestRepository,
        IPasswordHasher<Guest> passwordHasher,
        IOtpService otpService)
    {
        _guestRepository = guestRepository;
        _passwordHasher = passwordHasher;
        _otpService = otpService;
    }

    public async Task<IEnumerable<GuestResponseDTO>> GetAllGuestsAsync()
    {
        var guests = await _guestRepository.GetAllAsync();
        return guests.Select(MapToDTO);
    }

    public async Task<GuestResponseDTO?> GetGuestByIdAsync(int id)
    {
        var guest = await _guestRepository.GetByIdAsync(id);
        return guest == null ? null : MapToDTO(guest);
    }

    public async Task<GuestResponseDTO?> GetGuestByEmailAsync(string email)
    {
        var guest = await _guestRepository.GetByEmailAsync(email);
        return guest == null ? null : MapToDTO(guest);
    }

    public async Task<RegisterResponseDTO> RegisterGuestAsync(CreateGuestDTO dto)
    {
        var existingEmail = await _guestRepository.GetByEmailAsync(dto.Email);
        if (existingEmail != null)
        {
            throw new InvalidOperationException($"Guest with email '{dto.Email}' already exists.");
        }

        var existingPhone = await _guestRepository.GetByPhoneAsync(dto.PhoneNumber);
        if (existingPhone != null)
        {
            throw new InvalidOperationException($"Guest with phone number '{dto.PhoneNumber}' already exists.");
        }

        var nextId = await _guestRepository.GetNextGuestIdAsync();
        var guest = new Guest
        {
            GuestId = nextId,
            FullName = dto.FullName,
            Email = dto.Email,
            PhoneNumber = dto.PhoneNumber,
            Address = dto.Address,
            EmailVerified = false,
            PhoneVerified = false,
            IsActive = false,
            CreatedAt = DateTime.UtcNow
        };

        guest.PasswordHash = _passwordHasher.HashPassword(guest, dto.Password);
        await _guestRepository.CreateAsync(guest);

        // Send OTPs
        await _otpService.SendEmailOtpAsync(guest.Id, guest.Email);
        await _otpService.SendPhoneOtpAsync(guest.Id, guest.PhoneNumber);

        return new RegisterResponseDTO
        {
            GuestId = guest.GuestId,
            FullName = guest.FullName,
            Email = guest.Email,
            PhoneNumber = guest.PhoneNumber,
            EmailVerified = false,
            PhoneVerified = false,
            IsActive = false,
            Message = "Registration successful! Verification OTPs sent to email and phone. Verify both to activate your account."
        };
    }

    public async Task<GuestResponseDTO?> ValidateGuestLoginAsync(GuestLoginDTO dto)
    {
        var guest = await _guestRepository.GetByEmailAsync(dto.Email);
        if (guest == null) return null;

        var result = _passwordHasher.VerifyHashedPassword(guest, guest.PasswordHash, dto.Password);
        if (result == PasswordVerificationResult.Failed) return null;

        return MapToDTO(guest);
    }

    public async Task UpdateGuestAsync(UpdateGuestDTO dto)
    {
        var guest = await _guestRepository.GetByIdAsync(dto.GuestId);
        if (guest == null)
        {
            throw new KeyNotFoundException($"Guest with ID {dto.GuestId} not found.");
        }

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

    private static GuestResponseDTO MapToDTO(Guest g) => new()
    {
        GuestId = g.GuestId,
        FullName = g.FullName,
        Email = g.Email,
        PhoneNumber = g.PhoneNumber,
        Address = g.Address,
        EmailVerified = g.EmailVerified,
        PhoneVerified = g.PhoneVerified,
        IsActive = g.IsActive,
        CreatedAt = g.CreatedAt
    };
}
