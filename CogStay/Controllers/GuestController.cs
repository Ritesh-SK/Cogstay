using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using CogStay.Application.DTOs;
using CogStay.Domain.Enums;

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
            return RedirectToAction(nameof(VerifyOtp), new { email = dto.Email, phone = dto.PhoneNumber });
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(dto);
        }
    }

    [HttpGet]
    public IActionResult VerifyOtp(string? email, string? phone)
    {
        var e = !string.IsNullOrEmpty(email) ? email : TempData["UserEmail"]?.ToString();
        var p = !string.IsNullOrEmpty(phone) ? phone : TempData["UserPhone"]?.ToString();

        if (TempData["UserEmail"] != null) TempData.Keep("UserEmail");
        if (TempData["UserPhone"] != null) TempData.Keep("UserPhone");

        ViewBag.Email = e ?? "";
        ViewBag.Phone = p ?? "";
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> VerifyEmail(VerifyEmailOtpDTO dto)
    {
        if (TempData["UserEmail"] != null) TempData.Keep("UserEmail");
        if (TempData["UserPhone"] != null) TempData.Keep("UserPhone");

        try
        {
            var res = await _httpClient.PostAsJsonOrThrowAsync<OtpResultDTO, VerifyEmailOtpDTO>("api/auth/verify-email", dto);
            TempData["Success"] = res.Message;
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
        }
        return RedirectToAction(nameof(VerifyOtp), new { email = dto.Email, phone = TempData["UserPhone"]?.ToString() });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> VerifyPhone(VerifyPhoneOtpDTO dto)
    {
        if (TempData["UserEmail"] != null) TempData.Keep("UserEmail");
        if (TempData["UserPhone"] != null) TempData.Keep("UserPhone");

        try
        {
            var res = await _httpClient.PostAsJsonOrThrowAsync<OtpResultDTO, VerifyPhoneOtpDTO>("api/auth/verify-phone", dto);
            TempData["Success"] = res.Message;
            if (res.IsAccountActivated)
            {
                TempData["Success"] = "Account activated successfully! You may now sign in to your guest account.";
                return RedirectToAction(nameof(Login));
            }
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
        }
        return RedirectToAction(nameof(VerifyOtp), new { email = TempData["UserEmail"]?.ToString(), phone = dto.PhoneNumber });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResendOtp(ResendOtpDTO dto)
    {
        try
        {
            await _httpClient.PostAsJsonOrThrowAsync<object, ResendOtpDTO>("api/auth/resend-otp", dto);
            TempData["Success"] = $"A new {dto.OtpType} verification OTP code has been dispatched.";
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
        }
        return RedirectToAction(nameof(VerifyOtp), new { 
            email = dto.OtpType == OtpType.Email ? dto.Target : TempData["UserEmail"]?.ToString(), 
            phone = dto.OtpType == OtpType.Phone ? dto.Target : TempData["UserPhone"]?.ToString() 
        });
    }

    [HttpGet]
    public async Task<IActionResult> Dashboard()
    {
        int? guestId = HttpContext.Session.GetInt32("GuestId");
        if (!guestId.HasValue) return RedirectToAction(nameof(Login));

        ViewData["Role"] = "Guest";

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

        ViewData["Role"] = "Guest";
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

        ViewData["Role"] = "Guest";

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

        ViewData["Role"] = "Guest";
        var reservations = await _httpClient.GetFromJsonOrThrowAsync<IEnumerable<ReservationResponseDTO>>($"api/reservations/guest/{guestId.Value}", HttpContext);
        return View(reservations);
    }

    [HttpGet]
    public async Task<IActionResult> Billing()
    {
        int? guestId = HttpContext.Session.GetInt32("GuestId");
        if (!guestId.HasValue) return RedirectToAction(nameof(Login));

        ViewData["Role"] = "Guest";
        var bills = await _httpClient.GetFromJsonOrThrowAsync<IEnumerable<BillingResponseDTO>>("api/billing", HttpContext);
        return View(bills);
    }

    [HttpGet]
    public async Task<IActionResult> Profile()
    {
        int? guestId = HttpContext.Session.GetInt32("GuestId");
        if (!guestId.HasValue) return RedirectToAction(nameof(Login));

        ViewData["Role"] = "Guest";
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

        ViewData["Role"] = "Guest";

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
