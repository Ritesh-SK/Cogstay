using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using CogStayMVC.DTOs;
using CogStayMVC.Controllers.Api;

namespace CogStayMVC.Controllers;

public class CheckInController : Controller
{
    private readonly CheckInApiController _checkInApiController;
    private readonly ReservationApiController _reservationApiController;

    public CheckInController(
        CheckInApiController checkInApiController,
        ReservationApiController reservationApiController)
    {
        _checkInApiController = checkInApiController;
        _reservationApiController = reservationApiController;
    }

    [HttpGet]
    public async Task<IActionResult> ActiveStays()
    {
        string? staffRole = HttpContext.Session.GetString("StaffRole");
        if (string.IsNullOrEmpty(staffRole) || staffRole != "FrontDesk")
        {
            if (string.IsNullOrEmpty(staffRole)) return RedirectToAction("Login", "Staff");
            return RedirectToAction("Dashboard", "Staff", new { role = staffRole });
        }

        ViewData["Role"] = "FrontDesk";
        var stays = ControllerExtensions.Unpack(await _checkInApiController.GetAllStays());
        return View(stays);
    }

    [HttpGet]
    public async Task<IActionResult> CheckIn()
    {
        string? staffRole = HttpContext.Session.GetString("StaffRole");
        if (string.IsNullOrEmpty(staffRole) || staffRole != "FrontDesk")
        {
            if (string.IsNullOrEmpty(staffRole)) return RedirectToAction("Login", "Staff");
            return RedirectToAction("Dashboard", "Staff", new { role = staffRole });
        }

        ViewData["Role"] = "FrontDesk";
        ViewBag.Reservations = ControllerExtensions.Unpack(await _reservationApiController.GetAllReservations());
        return View(new CreateCheckInDTO());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CheckIn(CreateCheckInDTO dto)
    {
        string? staffRole = HttpContext.Session.GetString("StaffRole");
        if (string.IsNullOrEmpty(staffRole) || staffRole != "FrontDesk")
        {
            if (string.IsNullOrEmpty(staffRole)) return RedirectToAction("Login", "Staff");
            return RedirectToAction("Dashboard", "Staff", new { role = staffRole });
        }

        ViewData["Role"] = "FrontDesk";
        if (!ModelState.IsValid)
        {
            ViewBag.Reservations = ControllerExtensions.Unpack(await _reservationApiController.GetAllReservations());
            return View(dto);
        }

        try
        {
            ControllerExtensions.Unpack(await _checkInApiController.CheckInGuest(dto));
            TempData["Success"] = "Guest checked in successfully! Room status updated to Occupied.";
            return RedirectToAction(nameof(ActiveStays), new { role = "FrontDesk" });
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            ViewBag.Reservations = ControllerExtensions.Unpack(await _reservationApiController.GetAllReservations());
            return View(dto);
        }
    }

    [HttpGet]
    public async Task<IActionResult> CheckOut(int? id)
    {
        string? staffRole = HttpContext.Session.GetString("StaffRole");
        if (string.IsNullOrEmpty(staffRole) || staffRole != "FrontDesk")
        {
            if (string.IsNullOrEmpty(staffRole)) return RedirectToAction("Login", "Staff");
            return RedirectToAction("Dashboard", "Staff", new { role = staffRole });
        }

        ViewData["Role"] = "FrontDesk";
        var stays = ControllerExtensions.Unpack(await _checkInApiController.GetAllStays());
        ViewBag.Stays = stays;
        return View(new CheckOutDTO { StayId = id ?? 0 });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CheckOut(CheckOutDTO dto)
    {
        string? staffRole = HttpContext.Session.GetString("StaffRole");
        if (string.IsNullOrEmpty(staffRole) || staffRole != "FrontDesk")
        {
            if (string.IsNullOrEmpty(staffRole)) return RedirectToAction("Login", "Staff");
            return RedirectToAction("Dashboard", "Staff", new { role = staffRole });
        }

        ViewData["Role"] = "FrontDesk";
        if (!ModelState.IsValid)
        {
            ViewBag.Stays = ControllerExtensions.Unpack(await _checkInApiController.GetAllStays());
            return View(dto);
        }

        try
        {
            // Front Desk initiates checkout -> redirects to Front Desk Billing Module for final payment & cleaning request creation!
            return RedirectToAction("Payment", "Billing", new { stayId = dto.StayId, role = "FrontDesk" });
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            ViewBag.Stays = ControllerExtensions.Unpack(await _checkInApiController.GetAllStays());
            return View(dto);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        string? staffRole = HttpContext.Session.GetString("StaffRole");
        if (string.IsNullOrEmpty(staffRole) || staffRole != "FrontDesk")
        {
            if (string.IsNullOrEmpty(staffRole)) return RedirectToAction("Login", "Staff");
            return RedirectToAction("Dashboard", "Staff", new { role = staffRole });
        }

        ViewData["Role"] = "FrontDesk";
        try
        {
            ControllerExtensions.Unpack(await _checkInApiController.DeleteStay(id));
            TempData["Success"] = "Stay record deleted.";
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
        }
        return RedirectToAction(nameof(ActiveStays), new { role = "FrontDesk" });
    }
}
