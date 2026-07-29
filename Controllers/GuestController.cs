using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using CogStayMVC.DTOs;
using CogStayMVC.Controllers.Api;

namespace CogStayMVC.Controllers;

public class GuestController : Controller
{
    private readonly GuestApiController _guestApiController;
    private readonly RoomApiController _roomApiController;
    private readonly ReservationApiController _reservationApiController;
    private readonly CheckInApiController _checkInApiController;
    private readonly BillingApiController _billingApiController;

    public GuestController(
        GuestApiController guestApiController,
        RoomApiController roomApiController,
        ReservationApiController reservationApiController,
        CheckInApiController checkInApiController,
        BillingApiController billingApiController)
    {
        _guestApiController = guestApiController;
        _roomApiController = roomApiController;
        _reservationApiController = reservationApiController;
        _checkInApiController = checkInApiController;
        _billingApiController = billingApiController;
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
            var guest = ControllerExtensions.Unpack(await _guestApiController.LoginGuest(dto));
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
            var guest = ControllerExtensions.Unpack(await _guestApiController.RegisterGuest(dto));
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

        var guest = ControllerExtensions.Unpack(await _guestApiController.GetGuestById(guestId.Value));
        if (guest == null) return RedirectToAction(nameof(Login));

        ViewBag.GuestName = guest.FullName;
        HttpContext.Session.SetString("GuestName", guest.FullName);
        HttpContext.Session.SetString("GuestEmail", guest.Email);

        var reservations = ControllerExtensions.Unpack(await _reservationApiController.GetReservationsByGuest(guestId.Value));
        return View(reservations);
    }

    [HttpGet]
    public async Task<IActionResult> AvailableRooms()
    {
        int? guestId = HttpContext.Session.GetInt32("GuestId");
        if (!guestId.HasValue) return RedirectToAction(nameof(Login));

        ViewData["Role"] = "Guest";
        var rooms = ControllerExtensions.Unpack(await _roomApiController.GetAvailableRooms());
        return View(rooms);
    }

    [HttpGet]
    public async Task<IActionResult> BookRoom(int? roomId)
    {
        int? guestId = HttpContext.Session.GetInt32("GuestId");
        if (!guestId.HasValue) return RedirectToAction(nameof(Login));

        ViewBag.AvailableRooms = ControllerExtensions.Unpack(await _roomApiController.GetAvailableRooms());
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
            ViewBag.AvailableRooms = ControllerExtensions.Unpack(await _roomApiController.GetAvailableRooms());
            return View(dto);
        }

        try
        {
            ControllerExtensions.Unpack(await _reservationApiController.BookRoom(dto));
            TempData["Success"] = "Room booked successfully!";
            return RedirectToAction(nameof(MyReservations));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            ViewBag.AvailableRooms = ControllerExtensions.Unpack(await _roomApiController.GetAvailableRooms());
            return View(dto);
        }
    }

    [HttpGet]
    public async Task<IActionResult> MyReservations()
    {
        int? guestId = HttpContext.Session.GetInt32("GuestId");
        if (!guestId.HasValue) return RedirectToAction(nameof(Login));

        var reservations = ControllerExtensions.Unpack(await _reservationApiController.GetReservationsByGuest(guestId.Value));
        return View(reservations);
    }

    [HttpGet]
    public async Task<IActionResult> BookingHistory()
    {
        int? guestId = HttpContext.Session.GetInt32("GuestId");
        if (!guestId.HasValue) return RedirectToAction(nameof(Login));

        var reservations = ControllerExtensions.Unpack(await _reservationApiController.GetReservationsByGuest(guestId.Value));
        return View(reservations);
    }


    [HttpGet]
    public async Task<IActionResult> Billing()
    {
        int? guestId = HttpContext.Session.GetInt32("GuestId");
        if (!guestId.HasValue) return RedirectToAction(nameof(Login));

        var bills = ControllerExtensions.Unpack(await _billingApiController.GetAllBills());
        return View(bills);
    }

    [HttpGet]
    public async Task<IActionResult> Profile()
    {
        int? guestId = HttpContext.Session.GetInt32("GuestId");
        if (!guestId.HasValue) return RedirectToAction(nameof(Login));

        var guest = ControllerExtensions.Unpack(await _guestApiController.GetGuestById(guestId.Value));
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
            ControllerExtensions.Unpack(await _guestApiController.UpdateGuest(dto.GuestId, dto));
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
