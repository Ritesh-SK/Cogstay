using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using CogStay.Application.DTOs;

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
    public async Task<IActionResult> Login(LoginRequestDTO dto)
    {
        if (!ModelState.IsValid) return View(dto);

        try
        {
            var auth = await _httpClient.PostAsJsonOrThrowAsync<AuthResponseDTO, LoginRequestDTO>("api/auth/login", dto);
            if (auth == null)
            {
                ModelState.AddModelError(string.Empty, "Invalid email or password.");
                return View(dto);
            }

            HttpContext.Session.SetString("JwtToken", auth.Token);
            HttpContext.Session.SetString("RefreshToken", auth.RefreshToken);
            HttpContext.Session.SetInt32("GuestId", auth.IntegerId);
            HttpContext.Session.SetString("GuestName", auth.FullName);
            HttpContext.Session.SetString("GuestEmail", auth.Email);

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
            var reg = await _httpClient.PostAsJsonOrThrowAsync<RegisterResponseDTO, CreateGuestDTO>("api/auth/register", dto);
            TempData["Success"] = reg.Message;
            TempData["UserEmail"] = dto.Email;
            TempData["UserPhone"] = dto.PhoneNumber;
            return RedirectToAction(nameof(VerifyOtp));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(dto);
        }
    }

    [HttpGet]
    public IActionResult VerifyOtp()
    {
        ViewBag.Email = TempData["UserEmail"]?.ToString() ?? "";
        ViewBag.Phone = TempData["UserPhone"]?.ToString() ?? "";
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> VerifyEmail(VerifyEmailOtpDTO dto)
    {
        try
        {
            var res = await _httpClient.PostAsJsonOrThrowAsync<OtpResultDTO, VerifyEmailOtpDTO>("api/auth/verify-email", dto);
            TempData["Success"] = res.Message;
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
        }
        return RedirectToAction(nameof(VerifyOtp));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> VerifyPhone(VerifyPhoneOtpDTO dto)
    {
        try
        {
            var res = await _httpClient.PostAsJsonOrThrowAsync<OtpResultDTO, VerifyPhoneOtpDTO>("api/auth/verify-phone", dto);
            TempData["Success"] = res.Message;
            if (res.IsAccountActivated)
            {
                return RedirectToAction(nameof(Login));
            }
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
        }
        return RedirectToAction(nameof(VerifyOtp));
    }

    [HttpGet]
    public async Task<IActionResult> Dashboard()
    {
        int? guestId = HttpContext.Session.GetInt32("GuestId");
        if (!guestId.HasValue) return RedirectToAction(nameof(Login));

        var guest = await _httpClient.GetFromJsonOrThrowAsync<GuestResponseDTO>($"api/guests/{guestId.Value}", HttpContext);
        if (guest == null) return RedirectToAction(nameof(Login));

        ViewBag.GuestName = guest.FullName;
        HttpContext.Session.SetString("GuestName", guest.FullName);
        HttpContext.Session.SetString("GuestEmail", guest.Email);

        var reservations = await _httpClient.GetFromJsonOrThrowAsync<IEnumerable<ReservationResponseDTO>>($"api/reservations/guest/{guestId.Value}", HttpContext);
        return View(reservations);
    }

    [HttpGet]
    public async Task<IActionResult> AvailableRooms()
    {
        int? guestId = HttpContext.Session.GetInt32("GuestId");
        if (!guestId.HasValue) return RedirectToAction(nameof(Login));

        ViewData["Role"] = "Guest";
        var rooms = await _httpClient.GetFromJsonOrThrowAsync<IEnumerable<RoomResponseDTO>>("api/rooms/available", HttpContext);
        return View(rooms);
    }

    [HttpGet]
    public async Task<IActionResult> BookRoom(int? roomId)
    {
        int? guestId = HttpContext.Session.GetInt32("GuestId");
        if (!guestId.HasValue) return RedirectToAction(nameof(Login));

        ViewBag.AvailableRooms = await _httpClient.GetFromJsonOrThrowAsync<IEnumerable<RoomResponseDTO>>("api/rooms/available", HttpContext);
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
            ViewBag.AvailableRooms = await _httpClient.GetFromJsonOrThrowAsync<IEnumerable<RoomResponseDTO>>("api/rooms/available", HttpContext);
            return View(dto);
        }

        try
        {
            await _httpClient.PostAsJsonOrThrowAsync<ReservationResponseDTO, CreateReservationDTO>("api/reservations", dto, HttpContext);
            TempData["Success"] = "Room booked successfully!";
            return RedirectToAction(nameof(BookingHistory));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            ViewBag.AvailableRooms = await _httpClient.GetFromJsonOrThrowAsync<IEnumerable<RoomResponseDTO>>("api/rooms/available", HttpContext);
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

        var reservations = await _httpClient.GetFromJsonOrThrowAsync<IEnumerable<ReservationResponseDTO>>($"api/reservations/guest/{guestId.Value}", HttpContext);
        return View(reservations);
    }

    [HttpGet]
    public async Task<IActionResult> Billing()
    {
        int? guestId = HttpContext.Session.GetInt32("GuestId");
        if (!guestId.HasValue) return RedirectToAction(nameof(Login));

        var bills = await _httpClient.GetFromJsonOrThrowAsync<IEnumerable<BillingResponseDTO>>("api/billing", HttpContext);
        return View(bills);
    }

    [HttpGet]
    public async Task<IActionResult> Profile()
    {
        int? guestId = HttpContext.Session.GetInt32("GuestId");
        if (!guestId.HasValue) return RedirectToAction(nameof(Login));

        var guest = await _httpClient.GetFromJsonOrThrowAsync<GuestResponseDTO>($"api/guests/{guestId.Value}", HttpContext);
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
            await _httpClient.PutAsJsonOrThrowAsync($"api/guests/{dto.GuestId}", dto, HttpContext);
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
