using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using CogStayMVC.DTOs;

namespace CogStayMVC.Controllers;

public class BillingController : Controller
{
    private readonly HttpClient _httpClient;

    public BillingController(IHttpClientFactory httpClientFactory)
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
        var bills = await _httpClient.GetFromJsonOrThrowAsync<IEnumerable<BillingResponseDTO>>("api/billing");
        return View(bills);
    }

    [HttpGet]
    public async Task<IActionResult> Create(int? stayId)
    {
        string? staffRole = HttpContext.Session.GetString("StaffRole");
        if (string.IsNullOrEmpty(staffRole) || (staffRole != "FrontDesk" && staffRole != "Manager"))
        {
            if (string.IsNullOrEmpty(staffRole)) return RedirectToAction("Login", "Staff");
            return RedirectToAction("Dashboard", "Staff", new { role = staffRole });
        }

        ViewData["Role"] = staffRole;
        ViewBag.ActiveStays = await _httpClient.GetFromJsonOrThrowAsync<IEnumerable<StayRecordResponseDTO>>("api/stays");
        return View(new CreateBillDTO { StayId = stayId ?? 0 });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateBillDTO dto)
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
            ViewBag.ActiveStays = await _httpClient.GetFromJsonOrThrowAsync<IEnumerable<StayRecordResponseDTO>>("api/stays");
            return View(dto);
        }

        try
        {
            if (dto.TotalAmount > 0)
            {
                await _httpClient.PostAsJsonOrThrowAsync<BillingResponseDTO, CreateBillDTO>("api/billing", dto);
            }
            else
            {
                var remarksQuery = string.IsNullOrEmpty(dto.Remarks) ? "" : $"?remarks={Uri.EscapeDataString(dto.Remarks)}";
                await _httpClient.PostAsJsonOrThrowAsync<BillingResponseDTO, object>($"api/billing/generate/stay/{dto.StayId}{remarksQuery}", new { });
            }

            TempData["Success"] = "Bill generated successfully.";
            return RedirectToAction(nameof(Index), new { role = staffRole });
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            ViewBag.ActiveStays = await _httpClient.GetFromJsonOrThrowAsync<IEnumerable<StayRecordResponseDTO>>("api/stays");
            return View(dto);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Payment(int? stayId, int? billId)
    {
        string? staffRole = HttpContext.Session.GetString("StaffRole");
        if (string.IsNullOrEmpty(staffRole) || (staffRole != "FrontDesk" && staffRole != "Manager"))
        {
            if (string.IsNullOrEmpty(staffRole)) return RedirectToAction("Login", "Staff");
            return RedirectToAction("Dashboard", "Staff", new { role = staffRole });
        }

        ViewData["Role"] = staffRole;
        BillingResponseDTO? bill = null;

        if (billId.HasValue)
        {
            bill = await _httpClient.GetFromJsonOrThrowAsync<BillingResponseDTO>($"api/billing/{billId.Value}");
        }
        else if (stayId.HasValue)
        {
            try
            {
                bill = await _httpClient.GetFromJsonOrThrowAsync<BillingResponseDTO>($"api/billing/stay/{stayId.Value}");
            }
            catch
            {
                bill = await _httpClient.PostAsJsonOrThrowAsync<BillingResponseDTO, object>($"api/billing/generate/stay/{stayId.Value}", new { });
            }
        }

        if (bill == null)
        {
            TempData["Error"] = "No billing record found for this stay.";
            return RedirectToAction(nameof(Index), new { role = staffRole });
        }

        var dto = new ProcessPaymentDTO
        {
            BillId = bill.BillId,
            Remarks = "Payment accepted at Front Desk"
        };

        ViewBag.Bill = bill;
        return View(dto);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Payment(ProcessPaymentDTO dto)
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
            ViewBag.Bill = await _httpClient.GetFromJsonOrThrowAsync<BillingResponseDTO>($"api/billing/{dto.BillId}");
            return View(dto);
        }

        try
        {
            await _httpClient.PostAsJsonOrThrowAsync("api/billing/payment", dto);
            TempData["Success"] = "Payment accepted and checkout completed! Housekeeping cleaning request automatically generated.";
            return RedirectToAction(nameof(History), new { role = staffRole });
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            ViewBag.Bill = await _httpClient.GetFromJsonOrThrowAsync<BillingResponseDTO>($"api/billing/{dto.BillId}");
            return View(dto);
        }
    }

    [HttpGet]
    public async Task<IActionResult> History()
    {
        string? staffRole = HttpContext.Session.GetString("StaffRole");
        if (string.IsNullOrEmpty(staffRole) || (staffRole != "FrontDesk" && staffRole != "Manager"))
        {
            if (string.IsNullOrEmpty(staffRole)) return RedirectToAction("Login", "Staff");
            return RedirectToAction("Dashboard", "Staff", new { role = staffRole });
        }

        ViewData["Role"] = staffRole;
        var bills = await _httpClient.GetFromJsonOrThrowAsync<IEnumerable<BillingResponseDTO>>("api/billing");
        return View(bills);
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
            await _httpClient.DeleteOrThrowAsync($"api/billing/{id}");
            TempData["Success"] = "Bill record deleted.";
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
        }
        return RedirectToAction(nameof(Index), new { role = staffRole });
    }
}
