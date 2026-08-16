using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CogStay.Application.Contracts.Services;
using CogStay.Application.DTOs;

namespace CogStayApi.Controllers;

[ApiController]
[Route("api/billing")]
[Authorize]
public class BillingApiController : ControllerBase
{
    private readonly IBillingService _billingService;

    public BillingApiController(IBillingService billingService)
    {
        _billingService = billingService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<BillingResponseDTO>>> GetAllBills()
    {
        var allBills = await _billingService.GetAllBillsAsync();
        return Ok(allBills);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<BillingResponseDTO>> GetBillById(int id)
    {
        var bill = await _billingService.GetBillByIdAsync(id);
        if (bill == null)
        {
            return NotFound(new { message = $"Bill with ID {id} not found." });
        }
        return Ok(bill);
    }

    [HttpGet("stay/{stayId:int}")]
    public async Task<ActionResult<BillingResponseDTO>> GetBillByStayId(int stayId)
    {
        var bill = await _billingService.GetBillByStayIdAsync(stayId);
        if (bill == null)
        {
            return NotFound(new { message = $"Bill for stay ID {stayId} not found." });
        }
        return Ok(bill);
    }

    [HttpPost("generate/{stayId:int}")]
    [Authorize(Roles = "Admin,Manager,FrontDesk")]
    public async Task<ActionResult<BillingResponseDTO>> GenerateBillForStay(int stayId, [FromQuery] string? remarks = null)
    {
        try
        {
            var bill = await _billingService.GenerateBillForStayAsync(stayId, remarks);
            return Ok(bill);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpPost("process-payment")]
    public async Task<IActionResult> ProcessPayment([FromBody] ProcessPaymentDTO dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            await _billingService.ProcessPaymentAsync(dto);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> DeleteBill(int id)
    {
        try
        {
            var bill = await _billingService.GetBillByIdAsync(id);
            if (bill == null)
            {
                return NotFound(new { message = $"Bill with ID {id} not found." });
            }

            await _billingService.DeleteBillAsync(id);
            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while deleting the bill.", details = ex.Message });
        }
    }
}
