using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using CogStayMVC.DTOs;
using CogStayMVC.Services.Interfaces;

namespace CogStayMVC.Controllers.Api;

[ApiController]
[Route("api/staff")]
public class StaffApiController : ControllerBase
{
    private readonly IStaffService _staffService;

    public StaffApiController(IStaffService staffService)
    {
        _staffService = staffService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<StaffResponseDTO>>> GetAllStaff()
    {
        var staffList = await _staffService.GetAllStaffAsync();
        return Ok(staffList);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<StaffResponseDTO>> GetStaffById(int id)
    {
        var staff = await _staffService.GetStaffByIdAsync(id);
        if (staff == null)
        {
            return NotFound(new { message = $"Staff member with ID {id} not found." });
        }
        return Ok(staff);
    }

    [HttpPost]
    public async Task<ActionResult<StaffResponseDTO>> CreateStaff([FromBody] CreateStaffDTO dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var createdStaff = await _staffService.CreateStaffAsync(dto);
            return CreatedAtAction(nameof(GetStaffById), new { id = createdStaff.StaffId }, createdStaff);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while creating the staff member.", details = ex.Message });
        }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateStaff(int id, [FromBody] UpdateStaffDTO dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (id != dto.StaffId)
        {
            return BadRequest(new { message = "Staff ID in URL does not match ID in body." });
        }

        try
        {
            await _staffService.UpdateStaffAsync(dto);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while updating the staff member.", details = ex.Message });
        }
    }

    [HttpPost("login")]
    public async Task<ActionResult<StaffResponseDTO>> LoginStaff([FromBody] StaffLoginDTO dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var staff = await _staffService.ValidateStaffLoginAsync(dto);
        if (staff == null)
        {
            return Unauthorized(new { message = "Invalid email, password, or role." });
        }

        return Ok(staff);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteStaff(int id)
    {
        try
        {
            var staff = await _staffService.GetStaffByIdAsync(id);
            if (staff == null)
            {
                return NotFound(new { message = $"Staff member with ID {id} not found." });
            }

            await _staffService.DeleteStaffAsync(id);
            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while deleting the staff member.", details = ex.Message });
        }
    }
}
