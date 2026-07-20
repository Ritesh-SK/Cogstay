using System;
using System.Threading.Tasks;
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
        var bills = await _billingService.GetAllBillsAsync();
        return View(bills);
    }

    [HttpGet]
    public async Task<IActionResult> Create(int? stayId)
    {
        ViewBag.ActiveStays = await _checkInService.GetAllStaysAsync();
        return View(new CreateBillDTO { StayId = stayId ?? 0 });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateBillDTO dto)
    {
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

            TempData["Success"] = "Bill generated successfully by Front Desk.";
            return RedirectToAction(nameof(Index));
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
            return RedirectToAction(nameof(Index));
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
        if (!ModelState.IsValid)
        {
            ViewBag.Bill = await _billingService.GetBillByIdAsync(dto.BillId);
            return View(dto);
        }

        try
        {
            await _billingService.ProcessPaymentAsync(dto);
            TempData["Success"] = "Payment accepted and checkout completed! Housekeeping cleaning request automatically generated.";
            return RedirectToAction(nameof(History));
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
        var bills = await _billingService.GetAllBillsAsync();
        return View(bills);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await _billingService.DeleteBillAsync(id);
            TempData["Success"] = "Bill record deleted.";
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
        }
        return RedirectToAction(nameof(Index));
    }
}
