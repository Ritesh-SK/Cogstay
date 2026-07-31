using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using CogStayMVC.DTOs;
using CogStayMVC.Services.Interfaces;

namespace CogStayMVC.Controllers.Api;

[ApiController]
[Route("api/guests")]
public class GuestApiController : ControllerBase
{
    private readonly IGuestService _guestService;

    public GuestApiController(IGuestService guestService)
    {
        _guestService = guestService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<GuestResponseDTO>>> GetAllGuests()
    {
        var guests = await _guestService.GetAllGuestsAsync();
        return Ok(guests);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<GuestResponseDTO>> GetGuestById(int id)
    {
        var guest = await _guestService.GetGuestByIdAsync(id);
        if (guest == null)
        {
            return NotFound(new { message = $"Guest with ID {id} not found." });
        }
        return Ok(guest);
    }

    [HttpGet("email/{email}")]
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

    [HttpPost("register")]
    public async Task<ActionResult<GuestResponseDTO>> RegisterGuest([FromBody] CreateGuestDTO dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var registeredGuest = await _guestService.RegisterGuestAsync(dto);
            return CreatedAtAction(nameof(GetGuestById), new { id = registeredGuest.GuestId }, registeredGuest);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while registering the guest.", details = ex.Message });
        }
    }

    [HttpPost("login")]
    public async Task<ActionResult<GuestResponseDTO>> LoginGuest([FromBody] GuestLoginDTO dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var guest = await _guestService.ValidateGuestLoginAsync(dto);
        if (guest == null)
        {
            return Unauthorized(new { message = "Invalid email or password." });
        }

        return Ok(guest);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateGuest(int id, [FromBody] UpdateGuestDTO dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (id != dto.GuestId)
        {
            return BadRequest(new { message = "Guest ID in URL does not match ID in body." });
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
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while updating the guest.", details = ex.Message });
        }
    }

    [HttpDelete("{id:int}")]
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
}
