using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using CogStayMVC.DTOs;
using CogStayMVC.Services.Interfaces;

namespace CogStayMVC.Controllers.Api;

[ApiController]
[Route("api/housekeeping")]
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
    public async Task<ActionResult<HousekeepingTaskResponseDTO>> CreateTask([FromBody] CreateHousekeepingTaskDTO dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var createdTask = await _housekeepingService.CreateTaskAsync(dto);
            return CreatedAtAction(nameof(GetTaskById), new { id = createdTask.TaskId }, createdTask);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while creating the housekeeping task.", details = ex.Message });
        }
    }

    [HttpPut("status")]
    public async Task<IActionResult> UpdateTaskStatus([FromBody] UpdateTaskStatusDTO dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            await _housekeepingService.UpdateTaskStatusAsync(dto);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while updating the housekeeping task status.", details = ex.Message });
        }
    }

    [HttpDelete("{id:int}")]
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
            return StatusCode(500, new { message = "An error occurred while deleting the housekeeping task.", details = ex.Message });
        }
    }
}
