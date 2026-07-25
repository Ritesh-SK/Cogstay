using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using CogStayMVC.DTOs;
using CogStayMVC.Services.Interfaces;

namespace CogStayMVC.Controllers.Api;

[ApiController]
[Route("api/billing")]
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
        var bills = await _billingService.GetAllBillsAsync();
        return Ok(bills);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<BillingResponseDTO>> GetBillById(int id)
    {
        var bill = await _billingService.GetBillByIdAsync(id);
        if (bill == null)
        {
            return NotFound(new { message = $"Billing record with ID {id} not found." });
        }
        return Ok(bill);
    }

    [HttpGet("stay/{stayId:int}")]
    public async Task<ActionResult<BillingResponseDTO>> GetBillByStayId(int stayId)
    {
        var bill = await _billingService.GetBillByStayIdAsync(stayId);
        if (bill == null)
        {
            return NotFound(new { message = $"Billing record with Stay ID {stayId} not found." });
        }
        return Ok(bill);
    }

    [HttpPost]
    public async Task<ActionResult<BillingResponseDTO>> CreateBill([FromBody] CreateBillDTO dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var createdBill = await _billingService.CreateBillAsync(dto);
            return CreatedAtAction(nameof(GetBillById), new { id = createdBill.BillId }, createdBill);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while creating the bill.", details = ex.Message });
        }
    }

    [HttpPost("generate/stay/{stayId:int}")]
    public async Task<ActionResult<BillingResponseDTO>> GenerateBillForStay(int stayId, [FromQuery] string? remarks = null)
    {
        try
        {
            var generatedBill = await _billingService.GenerateBillForStayAsync(stayId, remarks);
            return CreatedAtAction(nameof(GetBillById), new { id = generatedBill.BillId }, generatedBill);
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
            return StatusCode(500, new { message = "An error occurred while generating the bill.", details = ex.Message });
        }
    }

    [HttpPost("payment")]
    public async Task<IActionResult> ProcessPayment([FromBody] ProcessPaymentDTO dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            await _billingService.ProcessPaymentAsync(dto);
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
            return StatusCode(500, new { message = "An error occurred while processing the payment.", details = ex.Message });
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteBill(int id)
    {
        try
        {
            var bill = await _billingService.GetBillByIdAsync(id);
            if (bill == null)
            {
                return NotFound(new { message = $"Billing record with ID {id} not found." });
            }

            await _billingService.DeleteBillAsync(id);
            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while deleting the billing record.", details = ex.Message });
        }
    }
}
