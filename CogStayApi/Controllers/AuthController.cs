using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CogStay.Application.Contracts.Services;
using CogStay.Application.DTOs;

namespace CogStayApi.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IOtpService _otpService;

    public AuthController(IAuthService authService, IOtpService otpService)
    {
        _authService = authService;
        _otpService = otpService;
    }

    [HttpPost("register")]
    public async Task<ActionResult<RegisterResponseDTO>> Register([FromBody] CreateGuestDTO dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            var response = await _authService.RegisterGuestAsync(dto);
            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred during registration.", details = ex.Message });
        }
    }

    [HttpPost("verify-email")]
    public async Task<ActionResult<OtpResultDTO>> VerifyEmail([FromBody] VerifyEmailOtpDTO dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var result = await _otpService.VerifyEmailOtpAsync(dto);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpPost("verify-phone")]
    public async Task<ActionResult<OtpResultDTO>> VerifyPhone([FromBody] VerifyPhoneOtpDTO dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var result = await _otpService.VerifyPhoneOtpAsync(dto);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpPost("resend-otp")]
    public async Task<IActionResult> ResendOtp([FromBody] ResendOtpDTO dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            await _otpService.ResendOtpAsync(dto);
            return Ok(new { message = "A new OTP has been dispatched successfully." });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error resending OTP.", details = ex.Message });
        }
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDTO>> Login([FromBody] LoginRequestDTO dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            var auth = await _authService.LoginGuestAsync(dto);
            return Ok(auth);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Login error.", details = ex.Message });
        }
    }

    [HttpPost("staff-login")]
    public async Task<ActionResult<AuthResponseDTO>> StaffLogin([FromBody] StaffLoginDTO dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            var auth = await _authService.LoginStaffAsync(dto);
            return Ok(auth);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Staff login error.", details = ex.Message });
        }
    }

    [HttpPost("refresh-token")]
    public async Task<ActionResult<AuthResponseDTO>> RefreshToken([FromBody] RefreshTokenRequestDTO dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            var auth = await _authService.RefreshTokenAsync(dto);
            return Ok(auth);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout([FromBody] RefreshTokenRequestDTO dto)
    {
        if (!string.IsNullOrWhiteSpace(dto.RefreshToken))
        {
            await _authService.RevokeTokenAsync(dto.RefreshToken);
        }
        return Ok(new { message = "Logged out successfully." });
    }
}
