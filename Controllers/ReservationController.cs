using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using CogStayMVC.DTOs;
using CogStayMVC.Services.Interfaces;

namespace CogStayMVC.Controllers;

public class ReservationController : Controller
{
    private readonly IReservationService _reservationService;
    private readonly IRoomService _roomService;
    private readonly IGuestService _guestService;

    public ReservationController(
        IReservationService reservationService,
        IRoomService roomService,
        IGuestService guestService)
    {
        _reservationService = reservationService;
        _roomService = roomService;
        _guestService = guestService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var reservations = await _reservationService.GetAllReservationsAsync();
        return View(reservations);
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var reservation = await _reservationService.GetReservationByIdAsync(id);
        if (reservation == null) return NotFound();
        return View(reservation);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        ViewBag.Guests = await _guestService.GetAllGuestsAsync();
        ViewBag.Rooms = await _roomService.GetAvailableRoomsAsync();
        return View(new CreateReservationDTO
        {
            CheckInDate = DateTime.Today,
            CheckOutDate = DateTime.Today.AddDays(1)
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateReservationDTO dto)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Guests = await _guestService.GetAllGuestsAsync();
            ViewBag.Rooms = await _roomService.GetAvailableRoomsAsync();
            return View(dto);
        }

        try
        {
            await _reservationService.BookRoomAsync(dto);
            TempData["Success"] = "Reservation created successfully!";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            ViewBag.Guests = await _guestService.GetAllGuestsAsync();
            ViewBag.Rooms = await _roomService.GetAvailableRoomsAsync();
            return View(dto);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int id)
    {
        try
        {
            await _reservationService.CancelReservationAsync(id);
            TempData["Success"] = "Reservation cancelled.";
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await _reservationService.DeleteReservationAsync(id);
            TempData["Success"] = "Reservation deleted.";
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
        }
        return RedirectToAction(nameof(Index));
    }
}
