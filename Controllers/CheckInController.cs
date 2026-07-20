using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using CogStayMVC.DTOs;
using CogStayMVC.Services.Interfaces;

namespace CogStayMVC.Controllers;

public class CheckInController : Controller
{
    private readonly ICheckInService _checkInService;
    private readonly IReservationService _reservationService;

    public CheckInController(
        ICheckInService checkInService,
        IReservationService reservationService)
    {
        _checkInService = checkInService;
        _reservationService = reservationService;
    }

    [HttpGet]
    public async Task<IActionResult> ActiveStays()
    {
        var stays = await _checkInService.GetAllStaysAsync();
        return View(stays);
    }

    [HttpGet]
    public async Task<IActionResult> CheckIn()
    {
        ViewBag.Reservations = await _reservationService.GetAllReservationsAsync();
        return View(new CreateCheckInDTO());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CheckIn(CreateCheckInDTO dto)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Reservations = await _reservationService.GetAllReservationsAsync();
            return View(dto);
        }

        try
        {
            await _checkInService.CheckInGuestAsync(dto);
            TempData["Success"] = "Guest checked in successfully! Room status updated to Occupied.";
            return RedirectToAction(nameof(ActiveStays));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            ViewBag.Reservations = await _reservationService.GetAllReservationsAsync();
            return View(dto);
        }
    }

    [HttpGet]
    public async Task<IActionResult> CheckOut(int? id)
    {
        var stays = await _checkInService.GetAllStaysAsync();
        ViewBag.Stays = stays;
        return View(new CheckOutDTO { StayId = id ?? 0 });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CheckOut(CheckOutDTO dto)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Stays = await _checkInService.GetAllStaysAsync();
            return View(dto);
        }

        try
        {
            // Front Desk initiates checkout -> redirects to Front Desk Billing Module for final payment & cleaning request creation!
            return RedirectToAction("Payment", "Billing", new { stayId = dto.StayId });
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            ViewBag.Stays = await _checkInService.GetAllStaysAsync();
            return View(dto);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await _checkInService.DeleteStayAsync(id);
            TempData["Success"] = "Stay record deleted.";
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
        }
        return RedirectToAction(nameof(ActiveStays));
    }
}
