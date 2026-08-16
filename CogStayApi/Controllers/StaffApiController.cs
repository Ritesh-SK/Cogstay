using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CogStay.Application.Contracts.Services;
using CogStay.Application.DTOs;

namespace CogStayApi.Controllers;

[ApiController]
[Route("api/staff")]
[Authorize(Roles = "Admin,Manager")]
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
        var staff = await _staffService.GetAllStaffAsync();
        return Ok(staff);
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
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<StaffResponseDTO>> CreateStaff([FromBody] CreateStaffDTO dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            var createdStaff = await _staffService.CreateStaffAsync(dto);
            return CreatedAtAction(nameof(GetStaffById), new { id = createdStaff.StaffId }, createdStaff);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateStaff(int id, [FromBody] UpdateStaffDTO dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        if (id != dto.StaffId) return BadRequest(new { message = "Staff ID mismatch." });

        try
        {
            await _staffService.UpdateStaffAsync(dto);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
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
