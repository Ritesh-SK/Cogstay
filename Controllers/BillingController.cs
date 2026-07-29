using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using CogStayMVC.DTOs;
using CogStayMVC.Controllers.Api;

namespace CogStayMVC.Controllers;

public class BillingController : Controller
{
    private readonly BillingApiController _billingApiController;
    private readonly CheckInApiController _checkInApiController;

    public BillingController(
        BillingApiController billingApiController,
        CheckInApiController checkInApiController)
    {
        _billingApiController = billingApiController;
        _checkInApiController = checkInApiController;
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
        var bills = ControllerExtensions.Unpack(await _billingApiController.GetAllBills());
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
        ViewBag.ActiveStays = ControllerExtensions.Unpack(await _checkInApiController.GetAllStays());
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
            ViewBag.ActiveStays = ControllerExtensions.Unpack(await _checkInApiController.GetAllStays());
            return View(dto);
        }

        try
        {
            if (dto.TotalAmount > 0)
            {
                ControllerExtensions.Unpack(await _billingApiController.CreateBill(dto));
            }
            else
            {
                ControllerExtensions.Unpack(await _billingApiController.GenerateBillForStay(dto.StayId, dto.Remarks));
            }

            TempData["Success"] = "Bill generated successfully.";
            return RedirectToAction(nameof(Index), new { role = staffRole });
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            ViewBag.ActiveStays = ControllerExtensions.Unpack(await _checkInApiController.GetAllStays());
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
            bill = ControllerExtensions.Unpack(await _billingApiController.GetBillById(billId.Value));
        }
        else if (stayId.HasValue)
        {
            try
            {
                bill = ControllerExtensions.Unpack(await _billingApiController.GetBillByStayId(stayId.Value));
            }
            catch
            {
                bill = ControllerExtensions.Unpack(await _billingApiController.GenerateBillForStay(stayId.Value, null));
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
            ViewBag.Bill = ControllerExtensions.Unpack(await _billingApiController.GetBillById(dto.BillId));
            return View(dto);
        }

        try
        {
            ControllerExtensions.Unpack(await _billingApiController.ProcessPayment(dto));
            TempData["Success"] = "Payment accepted and checkout completed! Housekeeping cleaning request automatically generated.";
            return RedirectToAction(nameof(History), new { role = staffRole });
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            ViewBag.Bill = ControllerExtensions.Unpack(await _billingApiController.GetBillById(dto.BillId));
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
        var bills = ControllerExtensions.Unpack(await _billingApiController.GetAllBills());
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
            ControllerExtensions.Unpack(await _billingApiController.DeleteBill(id));
            TempData["Success"] = "Bill record deleted.";
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
        }
        return RedirectToAction(nameof(Index), new { role = staffRole });
    }
}
