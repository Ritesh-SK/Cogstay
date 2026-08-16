using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using CogStay.Application.Contracts.Persistence;
using CogStay.Application.Contracts.Services;
using CogStay.Application.DTOs;
using CogStay.Domain.Entities;
using CogStay.Domain.Enums;

namespace CogStay.Application.Services;

public class AuthService : IAuthService
{
    private readonly IGuestService _guestService;
    private readonly IGuestRepository _guestRepository;
    private readonly IStaffRepository _staffRepository;
    private readonly IPasswordHasher<Guest> _guestPasswordHasher;
    private readonly IPasswordHasher<Staff> _staffPasswordHasher;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IRefreshTokenRepository _refreshTokenRepository;

    public AuthService(
        IGuestService guestService,
        IGuestRepository guestRepository,
        IStaffRepository staffRepository,
        IPasswordHasher<Guest> guestPasswordHasher,
        IPasswordHasher<Staff> staffPasswordHasher,
        IJwtTokenService jwtTokenService,
        IRefreshTokenRepository refreshTokenRepository)
    {
        _guestService = guestService;
        _guestRepository = guestRepository;
        _staffRepository = staffRepository;
        _guestPasswordHasher = guestPasswordHasher;
        _staffPasswordHasher = staffPasswordHasher;
        _jwtTokenService = jwtTokenService;
        _refreshTokenRepository = refreshTokenRepository;
    }

    public async Task<RegisterResponseDTO> RegisterGuestAsync(CreateGuestDTO dto)
    {
        return await _guestService.RegisterGuestAsync(dto);
    }

    public async Task<AuthResponseDTO> LoginGuestAsync(LoginRequestDTO dto)
    {
        var guest = await _guestRepository.GetByEmailAsync(dto.Email);
        if (guest == null)
        {
            throw new UnauthorizedAccessException("Invalid email or password.");
        }

        var result = _guestPasswordHasher.VerifyHashedPassword(guest, guest.PasswordHash, dto.Password);
        if (result == PasswordVerificationResult.Failed)
        {
            throw new UnauthorizedAccessException("Invalid email or password.");
        }

        if (!guest.EmailVerified || !guest.PhoneVerified || !guest.IsActive)
        {
            throw new InvalidOperationException("Account is pending verification. Please verify both Email OTP and Phone OTP to log in.");
        }

        var authResponse = _jwtTokenService.GenerateTokens(
            guest.Id,
            guest.GuestId,
            guest.FullName,
            guest.Email,
            StaffRole.Guest.ToString(),
            guest.EmailVerified,
            guest.PhoneVerified,
            guest.IsActive);

        var refreshTokenEntity = new RefreshToken
        {
            UserId = guest.Id,
            UserType = "Guest",
            Token = authResponse.RefreshToken,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            IsRevoked = false,
            CreatedAt = DateTime.UtcNow
        };
        await _refreshTokenRepository.CreateAsync(refreshTokenEntity);

        return authResponse;
    }

    public async Task<AuthResponseDTO> LoginStaffAsync(StaffLoginDTO dto)
    {
        var staff = await _staffRepository.GetByEmailAsync(dto.Email);
        if (staff == null || staff.Role != dto.Role || !staff.IsActive)
        {
            throw new UnauthorizedAccessException("Invalid credentials or inactive account.");
        }

        var result = _staffPasswordHasher.VerifyHashedPassword(staff, staff.PasswordHash, dto.Password);
        if (result == PasswordVerificationResult.Failed)
        {
            throw new UnauthorizedAccessException("Invalid credentials or inactive account.");
        }

        var authResponse = _jwtTokenService.GenerateTokens(
            staff.Id,
            staff.StaffId,
            staff.FullName,
            staff.Email,
            staff.Role.ToString(),
            emailVerified: true,
            phoneVerified: true,
            isActive: true);

        var refreshTokenEntity = new RefreshToken
        {
            UserId = staff.Id,
            UserType = "Staff",
            Token = authResponse.RefreshToken,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            IsRevoked = false,
            CreatedAt = DateTime.UtcNow
        };
        await _refreshTokenRepository.CreateAsync(refreshTokenEntity);

        return authResponse;
    }

    public async Task<AuthResponseDTO> RefreshTokenAsync(RefreshTokenRequestDTO dto)
    {
        var existingToken = await _refreshTokenRepository.GetByTokenAsync(dto.RefreshToken);
        if (existingToken == null || existingToken.IsRevoked || existingToken.ExpiresAt <= DateTime.UtcNow)
        {
            throw new UnauthorizedAccessException("Invalid or expired refresh token.");
        }

        var principal = _jwtTokenService.GetPrincipalFromExpiredToken(dto.AccessToken);
        if (principal == null)
        {
            throw new UnauthorizedAccessException("Invalid access token.");
        }

        string userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? existingToken.UserId;
        string email = principal.FindFirst(ClaimTypes.Email)?.Value ?? "";
        string role = principal.FindFirst(ClaimTypes.Role)?.Value ?? "Guest";
        string name = principal.FindFirst(ClaimTypes.Name)?.Value ?? "User";

        var newAuthResponse = _jwtTokenService.GenerateTokens(
            userId,
            0,
            name,
            email,
            role,
            emailVerified: true,
            phoneVerified: true,
            isActive: true);

        // Rotate refresh token
        string newTokenStr = newAuthResponse.RefreshToken;
        await _refreshTokenRepository.RevokeTokenAsync(existingToken.Token, newTokenStr);

        var newTokenEntity = new RefreshToken
        {
            UserId = userId,
            UserType = existingToken.UserType,
            Token = newTokenStr,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            IsRevoked = false,
            CreatedAt = DateTime.UtcNow
        };
        await _refreshTokenRepository.CreateAsync(newTokenEntity);

        return newAuthResponse;
    }

    public async Task RevokeTokenAsync(string token)
    {
        await _refreshTokenRepository.RevokeTokenAsync(token);
    }
}
