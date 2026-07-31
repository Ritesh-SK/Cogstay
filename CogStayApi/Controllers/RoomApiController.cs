using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using CogStayMVC.DTOs;
using CogStayMVC.Enums;
using CogStayMVC.Services.Interfaces;

namespace CogStayMVC.Controllers.Api;

[ApiController]
[Route("api/rooms")]
public class RoomApiController : ControllerBase
{
    private readonly IRoomService _roomService;

    public RoomApiController(IRoomService roomService)
    {
        _roomService = roomService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<RoomResponseDTO>>> GetAllRooms()
    {
        var rooms = await _roomService.GetAllRoomsAsync();
        return Ok(rooms);
    }

    [HttpGet("available")]
    public async Task<ActionResult<IEnumerable<RoomResponseDTO>>> GetAvailableRooms()
    {
        var rooms = await _roomService.GetAvailableRoomsAsync();
        return Ok(rooms);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<RoomResponseDTO>> GetRoomById(int id)
    {
        var room = await _roomService.GetRoomByIdAsync(id);
        if (room == null)
        {
            return NotFound(new { message = $"Room with ID {id} not found." });
        }
        return Ok(room);
    }

    [HttpPost]
    public async Task<ActionResult<RoomResponseDTO>> CreateRoom([FromBody] CreateRoomDTO dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var createdRoom = await _roomService.CreateRoomAsync(dto);
            return CreatedAtAction(nameof(GetRoomById), new { id = createdRoom.RoomId }, createdRoom);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while creating the room.", details = ex.Message });
        }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateRoom(int id, [FromBody] UpdateRoomDTO dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (id != dto.RoomId)
        {
            return BadRequest(new { message = "Room ID in URL does not match ID in body." });
        }

        try
        {
            await _roomService.UpdateRoomAsync(dto);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while updating the room.", details = ex.Message });
        }
    }

    [HttpPatch("{roomId:int}/status")]
    public async Task<IActionResult> UpdateRoomStatus(int roomId, [FromBody] RoomStatus status)
    {
        try
        {
            var room = await _roomService.GetRoomByIdAsync(roomId);
            if (room == null)
            {
                return NotFound(new { message = $"Room with ID {roomId} not found." });
            }

            await _roomService.UpdateRoomStatusAsync(roomId, status);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while updating the room status.", details = ex.Message });
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteRoom(int id)
    {
        try
        {
            var room = await _roomService.GetRoomByIdAsync(id);
            if (room == null)
            {
                return NotFound(new { message = $"Room with ID {id} not found." });
            }

            await _roomService.DeleteRoomAsync(id);
            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while deleting the room.", details = ex.Message });
        }
    }
}
