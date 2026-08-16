using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CogStay.Application.Contracts.Services;
using CogStay.Application.DTOs;

namespace CogStayApi.Controllers;

[ApiController]
[Route("api/housekeeping")]
[Authorize]
public class HousekeepingApiController : ControllerBase
{
    private readonly IHousekeepingService _housekeepingService;

    public HousekeepingApiController(IHousekeepingService housekeepingService)
    {
        _housekeepingService = housekeepingService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<HousekeepingTaskResponseDTO>>> GetAllTasks()
    {
        var tasks = await _housekeepingService.GetAllTasksAsync();
        return Ok(tasks);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<HousekeepingTaskResponseDTO>> GetTaskById(int id)
    {
        var task = await _housekeepingService.GetTaskByIdAsync(id);
        if (task == null)
        {
            return NotFound(new { message = $"Housekeeping task with ID {id} not found." });
        }
        return Ok(task);
    }

    [HttpGet("room/{roomId:int}")]
    public async Task<ActionResult<IEnumerable<HousekeepingTaskResponseDTO>>> GetTasksByRoomId(int roomId)
    {
        var tasks = await _housekeepingService.GetTasksByRoomIdAsync(roomId);
        return Ok(tasks);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Manager,FrontDesk,Housekeeping")]
    public async Task<ActionResult<HousekeepingTaskResponseDTO>> CreateTask([FromBody] CreateHousekeepingTaskDTO dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            var task = await _housekeepingService.CreateTaskAsync(dto);
            return CreatedAtAction(nameof(GetTaskById), new { id = task.TaskId }, task);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpPatch("status")]
    [Authorize(Roles = "Admin,Manager,Housekeeping")]
    public async Task<IActionResult> UpdateTaskStatus([FromBody] UpdateTaskStatusDTO dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            await _housekeepingService.UpdateTaskStatusAsync(dto);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> DeleteTask(int id)
    {
        try
        {
            var task = await _housekeepingService.GetTaskByIdAsync(id);
            if (task == null)
            {
                return NotFound(new { message = $"Housekeeping task with ID {id} not found." });
            }

            await _housekeepingService.DeleteTaskAsync(id);
            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while deleting the task.", details = ex.Message });
        }
    }
}
