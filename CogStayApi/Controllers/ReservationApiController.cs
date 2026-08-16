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
[Route("api/reservations")]
[Authorize]
public class ReservationApiController : ControllerBase
{
    private readonly IReservationService _reservationService;

    public ReservationApiController(IReservationService reservationService)
    {
        _reservationService = reservationService;
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Manager,FrontDesk")]
    public async Task<ActionResult<IEnumerable<ReservationResponseDTO>>> GetAllReservations()
    {
        var reservations = await _reservationService.GetAllReservationsAsync();
        return Ok(reservations);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ReservationResponseDTO>> GetReservationById(int id)
    {
        var reservation = await _reservationService.GetReservationByIdAsync(id);
        if (reservation == null)
        {
            return NotFound(new { message = $"Reservation with ID {id} not found." });
        }

        if (!IsAuthorizedForGuest(reservation.GuestId))
        {
            return Forbid();
        }

        return Ok(reservation);
    }

    [HttpGet("guest/{guestId:int}")]
    public async Task<ActionResult<IEnumerable<ReservationResponseDTO>>> GetReservationsByGuest(int guestId)
    {
        if (!IsAuthorizedForGuest(guestId))
        {
            return Forbid();
        }

        var reservations = await _reservationService.GetReservationsByGuestAsync(guestId);
        return Ok(reservations);
    }

    [HttpPost]
    public async Task<ActionResult<ReservationResponseDTO>> BookRoom([FromBody] CreateReservationDTO dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        if (!IsAuthorizedForGuest(dto.GuestId))
        {
            return Forbid();
        }

        try
        {
            var bookedReservation = await _reservationService.BookRoomAsync(dto);
            return CreatedAtAction(nameof(GetReservationById), new { id = bookedReservation.ReservationId }, bookedReservation);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while booking the room.", details = ex.Message });
        }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateReservation(int id, [FromBody] UpdateReservationDTO dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        if (id != dto.ReservationId) return BadRequest(new { message = "Reservation ID mismatch." });

        if (!IsAuthorizedForGuest(dto.GuestId))
        {
            return Forbid();
        }

        try
        {
            await _reservationService.UpdateReservationAsync(dto);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{id:int}/cancel")]
    public async Task<IActionResult> CancelReservation(int id)
    {
        var reservation = await _reservationService.GetReservationByIdAsync(id);
        if (reservation == null)
        {
            return NotFound(new { message = $"Reservation with ID {id} not found." });
        }

        if (!IsAuthorizedForGuest(reservation.GuestId))
        {
            return Forbid();
        }

        try
        {
            await _reservationService.CancelReservationAsync(id);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> DeleteReservation(int id)
    {
        try
        {
            var reservation = await _reservationService.GetReservationByIdAsync(id);
            if (reservation == null)
            {
                return NotFound(new { message = $"Reservation with ID {id} not found." });
            }

            await _reservationService.DeleteReservationAsync(id);
            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while deleting the reservation.", details = ex.Message });
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
