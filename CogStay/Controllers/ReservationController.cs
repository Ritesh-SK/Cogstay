using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using CogStayMVC.DTOs;

namespace CogStayMVC.Controllers;

public class ReservationController : Controller
{
    private readonly HttpClient _httpClient;

    public ReservationController(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient("CogStayApi");
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        string? staffRole = HttpContext.Session.GetString("StaffRole");
        if (string.IsNullOrEmpty(staffRole) || (staffRole != "FrontDesk" && staffRole != "Manager"))
        {
            if (string.IsNullOrEmpty(staffRole)) return RedirectToAction("Login", "Staff");
            return RedirectToAction("Dashboard", "Staff", new { role = staffRole });
        }

        ViewData["Role"] = staffRole;
        var reservations = await _httpClient.GetFromJsonOrThrowAsync<IEnumerable<ReservationResponseDTO>>("api/reservations");
        return View(reservations);
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        string? staffRole = HttpContext.Session.GetString("StaffRole");
        if (string.IsNullOrEmpty(staffRole) || (staffRole != "FrontDesk" && staffRole != "Manager"))
        {
            if (string.IsNullOrEmpty(staffRole)) return RedirectToAction("Login", "Staff");
            return RedirectToAction("Dashboard", "Staff", new { role = staffRole });
        }

        ViewData["Role"] = staffRole;
        var reservation = await _httpClient.GetFromJsonOrThrowAsync<ReservationResponseDTO>($"api/reservations/{id}");
        if (reservation == null) return NotFound();
        return View(reservation);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        string? staffRole = HttpContext.Session.GetString("StaffRole");
        if (string.IsNullOrEmpty(staffRole) || (staffRole != "FrontDesk" && staffRole != "Manager"))
        {
            if (string.IsNullOrEmpty(staffRole)) return RedirectToAction("Login", "Staff");
            return RedirectToAction("Dashboard", "Staff", new { role = staffRole });
        }

        ViewData["Role"] = staffRole;
        ViewBag.Guests = await _httpClient.GetFromJsonOrThrowAsync<IEnumerable<GuestResponseDTO>>("api/guests");
        ViewBag.Rooms = await _httpClient.GetFromJsonOrThrowAsync<IEnumerable<RoomResponseDTO>>("api/rooms/available");
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
        string? staffRole = HttpContext.Session.GetString("StaffRole");
        if (string.IsNullOrEmpty(staffRole) || (staffRole != "FrontDesk" && staffRole != "Manager"))
        {
            if (string.IsNullOrEmpty(staffRole)) return RedirectToAction("Login", "Staff");
            return RedirectToAction("Dashboard", "Staff", new { role = staffRole });
        }

        ViewData["Role"] = staffRole;
        if (!ModelState.IsValid)
        {
            ViewBag.Guests = await _httpClient.GetFromJsonOrThrowAsync<IEnumerable<GuestResponseDTO>>("api/guests");
            ViewBag.Rooms = await _httpClient.GetFromJsonOrThrowAsync<IEnumerable<RoomResponseDTO>>("api/rooms/available");
            return View(dto);
        }

        try
        {
            await _httpClient.PostAsJsonOrThrowAsync<ReservationResponseDTO, CreateReservationDTO>("api/reservations", dto);
            TempData["Success"] = "Reservation created successfully!";
            return RedirectToAction(nameof(Index), new { role = staffRole });
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            ViewBag.Guests = await _httpClient.GetFromJsonOrThrowAsync<IEnumerable<GuestResponseDTO>>("api/guests");
            ViewBag.Rooms = await _httpClient.GetFromJsonOrThrowAsync<IEnumerable<RoomResponseDTO>>("api/rooms/available");
            return View(dto);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int id)
    {
        string? staffRole = HttpContext.Session.GetString("StaffRole");
        if (string.IsNullOrEmpty(staffRole) || (staffRole != "FrontDesk" && staffRole != "Manager"))
        {
            if (string.IsNullOrEmpty(staffRole)) return RedirectToAction("Login", "Staff");
            return RedirectToAction("Dashboard", "Staff", new { role = staffRole });
        }

        ViewData["Role"] = staffRole;
        try
        {
            await _httpClient.PostAsJsonOrThrowAsync<object, object>($"api/reservations/{id}/cancel", new { });
            TempData["Success"] = "Reservation cancelled.";
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
        }
        return RedirectToAction(nameof(Index), new { role = staffRole });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        string? staffRole = HttpContext.Session.GetString("StaffRole");
        if (string.IsNullOrEmpty(staffRole) || (staffRole != "FrontDesk" && staffRole != "Manager"))
        {
            if (string.IsNullOrEmpty(staffRole)) return RedirectToAction("Login", "Staff");
            return RedirectToAction("Dashboard", "Staff", new { role = staffRole });
        }

        ViewData["Role"] = staffRole;
        try
        {
            await _httpClient.DeleteOrThrowAsync($"api/reservations/{id}");
            TempData["Success"] = "Reservation deleted.";
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
        }
        return RedirectToAction(nameof(Index), new { role = staffRole });
    }
}
