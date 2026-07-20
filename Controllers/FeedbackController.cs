using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using CogStayMVC.DTOs;
using CogStayMVC.Services.Interfaces;

namespace CogStayMVC.Controllers;

public class FeedbackController : Controller
{
    private readonly IFeedbackService _feedbackService;

    public FeedbackController(IFeedbackService feedbackService)
    {
        _feedbackService = feedbackService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var feedbacks = await _feedbackService.GetAllFeedbacksAsync();
        return View(feedbacks);
    }

    [HttpGet]
    public IActionResult Create(int? reservationId)
    {
        int? guestId = HttpContext.Session.GetInt32("GuestId");
        if (!guestId.HasValue) return RedirectToAction("Login", "Guest");

        var dto = new CreateFeedbackDTO
        {
            GuestId = guestId.Value,
            ReservationId = reservationId,
            Rating = 5
        };
        return View(dto);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateFeedbackDTO dto)
    {
        int? guestId = HttpContext.Session.GetInt32("GuestId");
        if (guestId.HasValue) dto.GuestId = guestId.Value;

        if (!ModelState.IsValid) return View(dto);

        try
        {
            await _feedbackService.SubmitFeedbackAsync(dto);
            TempData["Success"] = "Thank you for your feedback!";
            return RedirectToAction("Dashboard", "Guest");
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(dto);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await _feedbackService.DeleteFeedbackAsync(id);
            TempData["Success"] = "Feedback removed.";
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
        }
        return RedirectToAction(nameof(Index));
    }
}
