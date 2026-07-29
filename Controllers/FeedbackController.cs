using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using CogStayMVC.DTOs;
using CogStayMVC.Controllers.Api;

namespace CogStayMVC.Controllers;

public class FeedbackController : Controller
{
    private readonly FeedbackApiController _feedbackApiController;

    public FeedbackController(FeedbackApiController feedbackApiController)
    {
        _feedbackApiController = feedbackApiController;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        string? staffRole = HttpContext.Session.GetString("StaffRole");
        if (string.IsNullOrEmpty(staffRole) || staffRole != "Manager")
        {
            if (string.IsNullOrEmpty(staffRole)) return RedirectToAction("Login", "Staff");
            return RedirectToAction("Dashboard", "Staff", new { role = staffRole });
        }

        ViewData["Role"] = "Manager";
        var feedbacks = ControllerExtensions.Unpack(await _feedbackApiController.GetAllFeedbacks());
        return View(feedbacks);
    }

    [HttpGet]
    public IActionResult Create(int? reservationId)
    {
        int? guestId = HttpContext.Session.GetInt32("GuestId");
        if (!guestId.HasValue) return RedirectToAction("Login", "Guest");

        ViewData["Role"] = "Guest";
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
        if (!guestId.HasValue) return RedirectToAction("Login", "Guest");
        dto.GuestId = guestId.Value;

        ViewData["Role"] = "Guest";
        if (!ModelState.IsValid) return View(dto);

        try
        {
            ControllerExtensions.Unpack(await _feedbackApiController.SubmitFeedback(dto));
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
        string? staffRole = HttpContext.Session.GetString("StaffRole");
        if (string.IsNullOrEmpty(staffRole) || staffRole != "Manager")
        {
            if (string.IsNullOrEmpty(staffRole)) return RedirectToAction("Login", "Staff");
            return RedirectToAction("Dashboard", "Staff", new { role = staffRole });
        }

        try
        {
            ControllerExtensions.Unpack(await _feedbackApiController.DeleteFeedback(id));
            TempData["Success"] = "Feedback removed.";
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
        }
        return RedirectToAction(nameof(Index), new { role = "Manager" });
    }
}
