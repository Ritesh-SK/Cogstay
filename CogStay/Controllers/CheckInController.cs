using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using CogStayMVC.DTOs;

namespace CogStayMVC.Controllers;

public class CheckInController : Controller
{
    private readonly HttpClient _httpClient;

    public CheckInController(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient("CogStayApi");
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
        var stays = await _httpClient.GetFromJsonOrThrowAsync<IEnumerable<StayRecordResponseDTO>>("api/stays");
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
        ViewBag.Reservations = await _httpClient.GetFromJsonOrThrowAsync<IEnumerable<ReservationResponseDTO>>("api/reservations");
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
            ViewBag.Reservations = await _httpClient.GetFromJsonOrThrowAsync<IEnumerable<ReservationResponseDTO>>("api/reservations");
            return View(dto);
        }

        try
        {
            await _httpClient.PostAsJsonOrThrowAsync<StayRecordResponseDTO, CreateCheckInDTO>("api/stays/checkin", dto);
            TempData["Success"] = "Guest checked in successfully! Room status updated to Occupied.";
            return RedirectToAction(nameof(ActiveStays), new { role = "FrontDesk" });
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            ViewBag.Reservations = await _httpClient.GetFromJsonOrThrowAsync<IEnumerable<ReservationResponseDTO>>("api/reservations");
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
        var stays = await _httpClient.GetFromJsonOrThrowAsync<IEnumerable<StayRecordResponseDTO>>("api/stays");
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
            ViewBag.Stays = await _httpClient.GetFromJsonOrThrowAsync<IEnumerable<StayRecordResponseDTO>>("api/stays");
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
            ViewBag.Stays = await _httpClient.GetFromJsonOrThrowAsync<IEnumerable<StayRecordResponseDTO>>("api/stays");
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
            await _httpClient.DeleteOrThrowAsync($"api/stays/{id}");
            TempData["Success"] = "Stay record deleted.";
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
        }
        return RedirectToAction(nameof(ActiveStays), new { role = "FrontDesk" });
    }
}
