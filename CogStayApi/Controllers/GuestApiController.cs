using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CogStay.Application.Contracts.Services;
using CogStay.Application.DTOs;

namespace CogStayApi.Controllers;

[ApiController]
[Route("api/guests")]
[Authorize]
public class GuestApiController : ControllerBase
{
    private readonly IGuestService _guestService;

    public GuestApiController(IGuestService guestService)
    {
        _guestService = guestService;
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Manager,FrontDesk")]
    public async Task<ActionResult<IEnumerable<GuestResponseDTO>>> GetAllGuests()
    {
        var guests = await _guestService.GetAllGuestsAsync();
        return Ok(guests);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<GuestResponseDTO>> GetGuestById(int id)
    {
        if (!IsAuthorizedForGuest(id))
        {
            return Forbid();
        }

        var guest = await _guestService.GetGuestByIdAsync(id);
        if (guest == null)
        {
            return NotFound(new { message = $"Guest with ID {id} not found." });
        }
        return Ok(guest);
    }

    [HttpGet("email/{email}")]
    [Authorize(Roles = "Admin,Manager,FrontDesk")]
    public async Task<ActionResult<GuestResponseDTO>> GetGuestByEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return BadRequest(new { message = "Email cannot be empty." });
        }

        var guest = await _guestService.GetGuestByEmailAsync(email);
        if (guest == null)
        {
            return NotFound(new { message = $"Guest with email {email} not found." });
        }
        return Ok(guest);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateGuest(int id, [FromBody] UpdateGuestDTO dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        if (id != dto.GuestId) return BadRequest(new { message = "Guest ID mismatch." });

        if (!IsAuthorizedForGuest(id))
        {
            return Forbid();
        }

        try
        {
            await _guestService.UpdateGuestAsync(dto);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> DeleteGuest(int id)
    {
        try
        {
            var guest = await _guestService.GetGuestByIdAsync(id);
            if (guest == null)
            {
                return NotFound(new { message = $"Guest with ID {id} not found." });
            }

            await _guestService.DeleteGuestAsync(id);
            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while deleting the guest.", details = ex.Message });
        }
    }

    private bool IsAuthorizedForGuest(int guestId)
    {
        var role = User.FindFirst(ClaimTypes.Role)?.Value;
        if (role == "Admin" || role == "Manager" || role == "FrontDesk") return true;

        var integerIdClaim = User.FindFirst("IntegerId")?.Value;
        return integerIdClaim != null && int.TryParse(integerIdClaim, out var claimId) && claimId == guestId;
    }
}
