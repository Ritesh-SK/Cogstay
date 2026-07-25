using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using CogStayMVC.DTOs;
using CogStayMVC.Services.Interfaces;

namespace CogStayMVC.Controllers.Api;

[ApiController]
[Route("api/reservations")]
public class ReservationApiController : ControllerBase
{
    private readonly IReservationService _reservationService;

    public ReservationApiController(IReservationService reservationService)
    {
        _reservationService = reservationService;
    }

    [HttpGet]
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
        return Ok(reservation);
    }

    [HttpGet("guest/{guestId:int}")]
    public async Task<ActionResult<IEnumerable<ReservationResponseDTO>>> GetReservationsByGuest(int guestId)
    {
        var reservations = await _reservationService.GetReservationsByGuestAsync(guestId);
        return Ok(reservations);
    }

    [HttpPost]
    public async Task<ActionResult<ReservationResponseDTO>> BookRoom([FromBody] CreateReservationDTO dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
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
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (id != dto.ReservationId)
        {
            return BadRequest(new { message = "Reservation ID in URL does not match ID in body." });
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
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while updating the reservation.", details = ex.Message });
        }
    }

    [HttpPost("{id:int}/cancel")]
    public async Task<IActionResult> CancelReservation(int id)
    {
        try
        {
            var reservation = await _reservationService.GetReservationByIdAsync(id);
            if (reservation == null)
            {
                return NotFound(new { message = $"Reservation with ID {id} not found." });
            }

            await _reservationService.CancelReservationAsync(id);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while cancelling the reservation.", details = ex.Message });
        }
    }

    [HttpDelete("{id:int}")]
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
}
