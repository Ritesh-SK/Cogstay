using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using CogStayMVC.DTOs;
using CogStayMVC.Services.Interfaces;

namespace CogStayMVC.Controllers.Api;

[ApiController]
[Route("api/stays")]
public class CheckInApiController : ControllerBase
{
    private readonly ICheckInService _checkInService;

    public CheckInApiController(ICheckInService checkInService)
    {
        _checkInService = checkInService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<StayRecordResponseDTO>>> GetAllStays()
    {
        var stays = await _checkInService.GetAllStaysAsync();
        return Ok(stays);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<StayRecordResponseDTO>> GetStayById(int id)
    {
        var stay = await _checkInService.GetStayByIdAsync(id);
        if (stay == null)
        {
            return NotFound(new { message = $"Stay record with ID {id} not found." });
        }
        return Ok(stay);
    }

    [HttpGet("reservation/{reservationId:int}")]
    public async Task<ActionResult<StayRecordResponseDTO>> GetStayByReservationId(int reservationId)
    {
        var stay = await _checkInService.GetStayByReservationIdAsync(reservationId);
        if (stay == null)
        {
            return NotFound(new { message = $"Stay record with reservation ID {reservationId} not found." });
        }
        return Ok(stay);
    }

    [HttpPost("checkin")]
    public async Task<ActionResult<StayRecordResponseDTO>> CheckInGuest([FromBody] CreateCheckInDTO dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var stay = await _checkInService.CheckInGuestAsync(dto);
            return CreatedAtAction(nameof(GetStayById), new { id = stay.StayId }, stay);
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
            return StatusCode(500, new { message = "An error occurred during check-in.", details = ex.Message });
        }
    }

    [HttpPost("{id:int}/request-checkout")]
    public async Task<IActionResult> RequestCheckOut(int id)
    {
        try
        {
            var stay = await _checkInService.GetStayByIdAsync(id);
            if (stay == null)
            {
                return NotFound(new { message = $"Stay record with ID {id} not found." });
            }

            await _checkInService.RequestCheckOutAsync(id);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while requesting checkout.", details = ex.Message });
        }
    }

    [HttpPost("{id:int}/complete-checkout")]
    public async Task<IActionResult> CompleteCheckOut(int id)
    {
        try
        {
            var stay = await _checkInService.GetStayByIdAsync(id);
            if (stay == null)
            {
                return NotFound(new { message = $"Stay record with ID {id} not found." });
            }

            await _checkInService.CompleteCheckOutAsync(id);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while completing checkout.", details = ex.Message });
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteStay(int id)
    {
        try
        {
            var stay = await _checkInService.GetStayByIdAsync(id);
            if (stay == null)
            {
                return NotFound(new { message = $"Stay record with ID {id} not found." });
            }

            await _checkInService.DeleteStayAsync(id);
            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while deleting the stay record.", details = ex.Message });
        }
    }
}
