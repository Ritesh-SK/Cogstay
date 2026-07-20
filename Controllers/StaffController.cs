using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using CogStayMVC.DTOs;
using CogStayMVC.Services.Interfaces;

namespace CogStayMVC.Controllers;

public class StaffController : Controller
{
    private readonly IStaffService _staffService;

    public StaffController(IStaffService staffService)
    {
        _staffService = staffService;
    }

    [HttpGet]
    public IActionResult Login() => View(new StaffLoginDTO());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(StaffLoginDTO dto)
    {
        if (!ModelState.IsValid) return View(dto);

        var staff = await _staffService.ValidateStaffLoginAsync(dto);
        if (staff == null)
        {
            ModelState.AddModelError(string.Empty, "Invalid credentials or unauthorized role access.");
            return View(dto);
        }

        HttpContext.Session.SetInt32("StaffId", staff.StaffId);
        HttpContext.Session.SetString("StaffName", staff.FullName);
        HttpContext.Session.SetString("StaffRole", staff.Role.ToString());

        return RedirectToAction(nameof(Dashboard), new { role = staff.Role.ToString() });
    }

    [HttpGet]
    public IActionResult Dashboard(string role = "Admin")
    {
        string sessionRole = HttpContext.Session.GetString("StaffRole") ?? role;
        ViewData["Role"] = sessionRole;
        ViewBag.StaffName = HttpContext.Session.GetString("StaffName") ?? "Staff Member";
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var staffList = await _staffService.GetAllStaffAsync();
        return View(staffList);
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var staff = await _staffService.GetStaffByIdAsync(id);
        if (staff == null) return NotFound();
        return View(staff);
    }

    [HttpGet]
    public IActionResult Create() => View(new CreateStaffDTO());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateStaffDTO dto)
    {
        if (!ModelState.IsValid) return View(dto);

        try
        {
            await _staffService.CreateStaffAsync(dto);
            TempData["Success"] = "Staff member created successfully!";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(dto);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var staff = await _staffService.GetStaffByIdAsync(id);
        if (staff == null) return NotFound();

        var dto = new UpdateStaffDTO
        {
            StaffId = staff.StaffId,
            FullName = staff.FullName,
            Email = staff.Email,
            PhoneNumber = staff.PhoneNumber,
            Role = staff.Role,
            IsActive = staff.IsActive
        };
        return View(dto);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, UpdateStaffDTO dto)
    {
        if (id != dto.StaffId) return BadRequest();
        if (!ModelState.IsValid) return View(dto);

        try
        {
            await _staffService.UpdateStaffAsync(dto);
            TempData["Success"] = "Staff updated successfully!";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(dto);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        var staff = await _staffService.GetStaffByIdAsync(id);
        if (staff == null) return NotFound();
        return View(staff);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        try
        {
            await _staffService.DeleteStaffAsync(id);
            TempData["Success"] = "Staff member deleted successfully!";
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction(nameof(Login));
    }
}
