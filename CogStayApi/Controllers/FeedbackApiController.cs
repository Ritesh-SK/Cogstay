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
[Route("api/feedback")]
[Authorize]
public class FeedbackApiController : ControllerBase
{
    private readonly IFeedbackService _feedbackService;

    public FeedbackApiController(IFeedbackService feedbackService)
    {
        _feedbackService = feedbackService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<FeedbackResponseDTO>>> GetAllFeedbacks()
    {
        var feedbacks = await _feedbackService.GetAllFeedbacksAsync();
        return Ok(feedbacks);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<FeedbackResponseDTO>> GetFeedbackById(int id)
    {
        var feedback = await _feedbackService.GetFeedbackByIdAsync(id);
        if (feedback == null)
        {
            return NotFound(new { message = $"Feedback with ID {id} not found." });
        }
        return Ok(feedback);
    }

    [HttpPost]
    public async Task<ActionResult<FeedbackResponseDTO>> SubmitFeedback([FromBody] CreateFeedbackDTO dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var role = User.FindFirst(ClaimTypes.Role)?.Value;
        if (role == "Guest")
        {
            var integerIdClaim = User.FindFirst("IntegerId")?.Value;
            if (integerIdClaim != null && int.TryParse(integerIdClaim, out var claimId) && claimId != dto.GuestId)
            {
                return Forbid();
            }
        }

        try
        {
            var feedback = await _feedbackService.SubmitFeedbackAsync(dto);
            return CreatedAtAction(nameof(GetFeedbackById), new { id = feedback.FeedbackId }, feedback);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> DeleteFeedback(int id)
    {
        try
        {
            var feedback = await _feedbackService.GetFeedbackByIdAsync(id);
            if (feedback == null)
            {
                return NotFound(new { message = $"Feedback with ID {id} not found." });
            }

            await _feedbackService.DeleteFeedbackAsync(id);
            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while deleting the feedback.", details = ex.Message });
        }
    }
}
