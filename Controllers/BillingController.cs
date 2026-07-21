using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using CogStayMVC.DTOs;
using CogStayMVC.Services.Interfaces;

namespace CogStayMVC.Controllers;

public class BillingController : Controller
{
    private readonly IBillingService _billingService;
    private readonly ICheckInService _checkInService;

    public BillingController(
        IBillingService billingService,
        ICheckInService checkInService)
    {
        _billingService = billingService;
        _checkInService = checkInService;
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
        var bills = await _billingService.GetAllBillsAsync();
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
        ViewBag.ActiveStays = await _checkInService.GetAllStaysAsync();
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
            ViewBag.ActiveStays = await _checkInService.GetAllStaysAsync();
            return View(dto);
        }

        try
        {
            if (dto.TotalAmount > 0)
            {
                await _billingService.CreateBillAsync(dto);
            }
            else
            {
                await _billingService.GenerateBillForStayAsync(dto.StayId, dto.Remarks);
            }

            TempData["Success"] = "Bill generated successfully.";
            return RedirectToAction(nameof(Index), new { role = staffRole });
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            ViewBag.ActiveStays = await _checkInService.GetAllStaysAsync();
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
            bill = await _billingService.GetBillByIdAsync(billId.Value);
        }
        else if (stayId.HasValue)
        {
            bill = await _billingService.GetBillByStayIdAsync(stayId.Value) 
                   ?? await _billingService.GenerateBillForStayAsync(stayId.Value);
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
            ViewBag.Bill = await _billingService.GetBillByIdAsync(dto.BillId);
            return View(dto);
        }

        try
        {
            await _billingService.ProcessPaymentAsync(dto);
            TempData["Success"] = "Payment accepted and checkout completed! Housekeeping cleaning request automatically generated.";
            return RedirectToAction(nameof(History), new { role = staffRole });
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            ViewBag.Bill = await _billingService.GetBillByIdAsync(dto.BillId);
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
        var bills = await _billingService.GetAllBillsAsync();
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
            await _billingService.DeleteBillAsync(id);
            TempData["Success"] = "Bill record deleted.";
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
        }
        return RedirectToAction(nameof(Index), new { role = staffRole });
    }
}
