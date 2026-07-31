using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using CogStayMVC.DTOs;

namespace CogStayMVC.Controllers;

public class GuestController : Controller
{
    private readonly HttpClient _httpClient;

    public GuestController(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient("CogStayApi");
    }

    [HttpGet]
    public IActionResult Login() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(GuestLoginDTO dto)
    {
        if (!ModelState.IsValid) return View(dto);

        try
        {
            var guest = await _httpClient.PostAsJsonOrThrowAsync<GuestResponseDTO, GuestLoginDTO>("api/guests/login", dto);
            if (guest == null)
            {
                ModelState.AddModelError(string.Empty, "Invalid email or password.");
                return View(dto);
            }

            HttpContext.Session.SetInt32("GuestId", guest.GuestId);
            HttpContext.Session.SetString("GuestName", guest.FullName);
            HttpContext.Session.SetString("GuestEmail", guest.Email);

            return RedirectToAction(nameof(Dashboard));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(dto);
        }
    }

    [HttpGet]
    public IActionResult Register() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(CreateGuestDTO dto)
    {
        if (!ModelState.IsValid) return View(dto);

        try
        {
            var guest = await _httpClient.PostAsJsonOrThrowAsync<GuestResponseDTO, CreateGuestDTO>("api/guests/register", dto);
            HttpContext.Session.SetInt32("GuestId", guest.GuestId);
            HttpContext.Session.SetString("GuestName", guest.FullName);
            HttpContext.Session.SetString("GuestEmail", guest.Email);

            TempData["Success"] = "Account registered successfully!";
            return RedirectToAction(nameof(Dashboard));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(dto);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Dashboard()
    {
        int? guestId = HttpContext.Session.GetInt32("GuestId");
        if (!guestId.HasValue) return RedirectToAction(nameof(Login));

        var guest = await _httpClient.GetFromJsonOrThrowAsync<GuestResponseDTO>($"api/guests/{guestId.Value}");
        if (guest == null) return RedirectToAction(nameof(Login));

        ViewBag.GuestName = guest.FullName;
        HttpContext.Session.SetString("GuestName", guest.FullName);
        HttpContext.Session.SetString("GuestEmail", guest.Email);

        var reservations = await _httpClient.GetFromJsonOrThrowAsync<IEnumerable<ReservationResponseDTO>>($"api/reservations/guest/{guestId.Value}");
        return View(reservations);
    }

    [HttpGet]
    public async Task<IActionResult> AvailableRooms()
    {
        int? guestId = HttpContext.Session.GetInt32("GuestId");
        if (!guestId.HasValue) return RedirectToAction(nameof(Login));

        ViewData["Role"] = "Guest";
        var rooms = await _httpClient.GetFromJsonOrThrowAsync<IEnumerable<RoomResponseDTO>>("api/rooms/available");
        return View(rooms);
    }

    [HttpGet]
    public async Task<IActionResult> BookRoom(int? roomId)
    {
        int? guestId = HttpContext.Session.GetInt32("GuestId");
        if (!guestId.HasValue) return RedirectToAction(nameof(Login));

        ViewBag.AvailableRooms = await _httpClient.GetFromJsonOrThrowAsync<IEnumerable<RoomResponseDTO>>("api/rooms/available");
        var dto = new CreateReservationDTO
        {
            GuestId = guestId.Value,
            RoomId = roomId ?? 0,
            CheckInDate = DateTime.Today,
            CheckOutDate = DateTime.Today.AddDays(1)
        };
        return View(dto);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> BookRoom(CreateReservationDTO dto)
    {
        int? guestId = HttpContext.Session.GetInt32("GuestId");
        if (!guestId.HasValue) return RedirectToAction(nameof(Login));
        dto.GuestId = guestId.Value;

        if (!ModelState.IsValid)
        {
            ViewBag.AvailableRooms = await _httpClient.GetFromJsonOrThrowAsync<IEnumerable<RoomResponseDTO>>("api/rooms/available");
            return View(dto);
        }

        try
        {
            await _httpClient.PostAsJsonOrThrowAsync<ReservationResponseDTO, CreateReservationDTO>("api/reservations", dto);
            TempData["Success"] = "Room booked successfully!";
            return RedirectToAction(nameof(BookingHistory));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            ViewBag.AvailableRooms = await _httpClient.GetFromJsonOrThrowAsync<IEnumerable<RoomResponseDTO>>("api/rooms/available");
            return View(dto);
        }
    }

    [HttpGet]
    public IActionResult MyReservations()
    {
        return RedirectToAction(nameof(BookingHistory));
    }

    [HttpGet]
    public async Task<IActionResult> BookingHistory()
    {
        int? guestId = HttpContext.Session.GetInt32("GuestId");
        if (!guestId.HasValue) return RedirectToAction(nameof(Login));

        var reservations = await _httpClient.GetFromJsonOrThrowAsync<IEnumerable<ReservationResponseDTO>>($"api/reservations/guest/{guestId.Value}");
        return View(reservations);
    }

    [HttpGet]
    public async Task<IActionResult> Billing()
    {
        int? guestId = HttpContext.Session.GetInt32("GuestId");
        if (!guestId.HasValue) return RedirectToAction(nameof(Login));

        var bills = await _httpClient.GetFromJsonOrThrowAsync<IEnumerable<BillingResponseDTO>>("api/billing");
        return View(bills);
    }

    [HttpGet]
    public async Task<IActionResult> Profile()
    {
        int? guestId = HttpContext.Session.GetInt32("GuestId");
        if (!guestId.HasValue) return RedirectToAction(nameof(Login));

        var guest = await _httpClient.GetFromJsonOrThrowAsync<GuestResponseDTO>($"api/guests/{guestId.Value}");
        if (guest == null) return NotFound();

        var dto = new UpdateGuestDTO
        {
            GuestId = guest.GuestId,
            FullName = guest.FullName,
            Email = guest.Email,
            PhoneNumber = guest.PhoneNumber,
            Address = guest.Address
        };
        return View(dto);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Profile(UpdateGuestDTO dto)
    {
        int? guestId = HttpContext.Session.GetInt32("GuestId");
        if (!guestId.HasValue) return RedirectToAction(nameof(Login));
        dto.GuestId = guestId.Value;

        if (!ModelState.IsValid) return View(dto);

        try
        {
            await _httpClient.PutAsJsonOrThrowAsync($"api/guests/{dto.GuestId}", dto);
            HttpContext.Session.SetString("GuestName", dto.FullName);
            HttpContext.Session.SetString("GuestEmail", dto.Email);
            TempData["Success"] = "Profile updated successfully!";
            return RedirectToAction(nameof(Profile));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(dto);
        }
    }

    [HttpGet]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction(nameof(Login));
    }
}
