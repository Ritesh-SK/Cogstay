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

public class StaffService : IStaffService
{
    private readonly IStaffRepository _staffRepository;
    private readonly IPasswordHasher<Staff> _passwordHasher;

    public StaffService(IStaffRepository staffRepository, IPasswordHasher<Staff> passwordHasher)
    {
        _staffRepository = staffRepository;
        _passwordHasher = passwordHasher;
    }

    public async Task<IEnumerable<StaffResponseDTO>> GetAllStaffAsync()
    {
        var staff = await _staffRepository.GetAllAsync();
        return staff.Select(MapToDTO);
    }

    public async Task<StaffResponseDTO?> GetStaffByIdAsync(int id)
    {
        var staff = await _staffRepository.GetByIdAsync(id);
        return staff == null ? null : MapToDTO(staff);
    }

    public async Task<StaffResponseDTO> CreateStaffAsync(CreateStaffDTO dto)
    {
        var existing = await _staffRepository.GetByEmailAsync(dto.Email);
        if (existing != null)
        {
            throw new InvalidOperationException($"Staff with email '{dto.Email}' already exists.");
        }

        var nextId = await _staffRepository.GetNextStaffIdAsync();
        var staff = new Staff
        {
            StaffId = nextId,
            FullName = dto.FullName,
            Email = dto.Email,
            PhoneNumber = dto.PhoneNumber,
            Role = dto.Role,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        staff.PasswordHash = _passwordHasher.HashPassword(staff, dto.Password);
        await _staffRepository.CreateAsync(staff);
        return MapToDTO(staff);
    }

    public async Task UpdateStaffAsync(UpdateStaffDTO dto)
    {
        var staff = await _staffRepository.GetByIdAsync(dto.StaffId);
        if (staff == null)
        {
            throw new KeyNotFoundException($"Staff with ID {dto.StaffId} not found.");
        }

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
        if (staff == null || staff.Role != dto.Role || !staff.IsActive) return null;

        var result = _passwordHasher.VerifyHashedPassword(staff, staff.PasswordHash, dto.Password);
        if (result == PasswordVerificationResult.Failed) return null;

        return MapToDTO(staff);
    }

    private static StaffResponseDTO MapToDTO(Staff s) => new()
    {
        StaffId = s.StaffId,
        FullName = s.FullName,
        Email = s.Email,
        PhoneNumber = s.PhoneNumber,
        Role = s.Role,
        IsActive = s.IsActive,
        CreatedAt = s.CreatedAt
    };
}
