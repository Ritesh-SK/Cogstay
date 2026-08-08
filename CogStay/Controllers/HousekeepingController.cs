using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using CogStayMVC.DTOs;

namespace CogStayMVC.Controllers;

public class HousekeepingController : Controller
{
    private readonly HttpClient _httpClient;

    public HousekeepingController(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient("CogStayApi");
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        string? staffRole = HttpContext.Session.GetString("StaffRole");
        if (string.IsNullOrEmpty(staffRole) || (staffRole != "Housekeeping" && staffRole != "Manager" && staffRole != "FrontDesk"))
        {
            if (string.IsNullOrEmpty(staffRole)) return RedirectToAction("Login", "Staff");
            return RedirectToAction("Dashboard", "Staff", new { role = staffRole });
        }

        ViewData["Role"] = staffRole;
        var tasks = await _httpClient.GetFromJsonOrThrowAsync<IEnumerable<HousekeepingTaskResponseDTO>>("api/housekeeping") ?? System.Linq.Enumerable.Empty<HousekeepingTaskResponseDTO>();

        try
        {
            var stays = await _httpClient.GetFromJsonOrThrowAsync<IEnumerable<StayRecordResponseDTO>>("api/stays") ?? System.Linq.Enumerable.Empty<StayRecordResponseDTO>();
            var roomGuestMap = stays
                .Where(s => !s.ActualCheckOut.HasValue)
                .GroupBy(s => s.RoomNumber)
                .ToDictionary(g => g.Key, g => g.First().GuestName);

            ViewBag.RoomGuestMap = roomGuestMap;
        }
        catch
        {
            ViewBag.RoomGuestMap = new Dictionary<string, string>();
        }

        return View(tasks);
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        string? staffRole = HttpContext.Session.GetString("StaffRole");
        if (string.IsNullOrEmpty(staffRole) || (staffRole != "Housekeeping" && staffRole != "Manager" && staffRole != "FrontDesk"))
        {
            if (string.IsNullOrEmpty(staffRole)) return RedirectToAction("Login", "Staff");
            return RedirectToAction("Dashboard", "Staff", new { role = staffRole });
        }

        ViewData["Role"] = staffRole;
        var task = await _httpClient.GetFromJsonOrThrowAsync<HousekeepingTaskResponseDTO>($"api/housekeeping/{id}");
        if (task == null) return NotFound();
        return View(task);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        string? staffRole = HttpContext.Session.GetString("StaffRole");
        if (string.IsNullOrEmpty(staffRole) || (staffRole != "Housekeeping" && staffRole != "Manager" && staffRole != "FrontDesk"))
        {
            if (string.IsNullOrEmpty(staffRole)) return RedirectToAction("Login", "Staff");
            return RedirectToAction("Dashboard", "Staff", new { role = staffRole });
        }

        ViewData["Role"] = staffRole;
        ViewBag.Rooms = await _httpClient.GetFromJsonOrThrowAsync<IEnumerable<RoomResponseDTO>>("api/rooms");
        return View(new CreateHousekeepingTaskDTO());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateHousekeepingTaskDTO dto)
    {
        string? staffRole = HttpContext.Session.GetString("StaffRole");
        if (string.IsNullOrEmpty(staffRole) || (staffRole != "Housekeeping" && staffRole != "Manager" && staffRole != "FrontDesk"))
        {
            if (string.IsNullOrEmpty(staffRole)) return RedirectToAction("Login", "Staff");
            return RedirectToAction("Dashboard", "Staff", new { role = staffRole });
        }

        ViewData["Role"] = staffRole;
        string? taskType = Request.Form["TaskType"];
        if (!string.IsNullOrEmpty(taskType))
        {
            dto.TaskDescription = $"[{taskType}] {dto.TaskDescription}";
        }

        if (!ModelState.IsValid)
        {
            ViewBag.Rooms = await _httpClient.GetFromJsonOrThrowAsync<IEnumerable<RoomResponseDTO>>("api/rooms");
            return View(dto);
        }

        try
        {
            await _httpClient.PostAsJsonOrThrowAsync<HousekeepingTaskResponseDTO, CreateHousekeepingTaskDTO>("api/housekeeping", dto);
            TempData["Success"] = "Housekeeping cleaning request created.";
            return RedirectToAction(nameof(Index), new { role = staffRole });
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            ViewBag.Rooms = await _httpClient.GetFromJsonOrThrowAsync<IEnumerable<RoomResponseDTO>>("api/rooms");
            return View(dto);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        string? staffRole = HttpContext.Session.GetString("StaffRole");
        if (string.IsNullOrEmpty(staffRole) || (staffRole != "Housekeeping" && staffRole != "Manager" && staffRole != "FrontDesk"))
        {
            if (string.IsNullOrEmpty(staffRole)) return RedirectToAction("Login", "Staff");
            return RedirectToAction("Dashboard", "Staff", new { role = staffRole });
        }

        ViewData["Role"] = staffRole;
        var task = await _httpClient.GetFromJsonOrThrowAsync<HousekeepingTaskResponseDTO>($"api/housekeeping/{id}");

        if (task == null)
        {
            return Content($"Task with ID {id} was not found in the database.");
        }

        var dto = new UpdateTaskStatusDTO
        {
            TaskId = task.TaskId,
            TaskStatus = task.TaskStatus
        };
        ViewBag.Task = task;
        return View(dto);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, UpdateTaskStatusDTO dto)
    {
        string? staffRole = HttpContext.Session.GetString("StaffRole");
        if (string.IsNullOrEmpty(staffRole) || (staffRole != "Housekeeping" && staffRole != "Manager" && staffRole != "FrontDesk"))
        {
            if (string.IsNullOrEmpty(staffRole)) return RedirectToAction("Login", "Staff");
            return RedirectToAction("Dashboard", "Staff", new { role = staffRole });
        }

        ViewData["Role"] = staffRole;
        if (id != dto.TaskId) return BadRequest();

        if (!ModelState.IsValid)
        {
            ViewBag.Task = await _httpClient.GetFromJsonOrThrowAsync<HousekeepingTaskResponseDTO>($"api/housekeeping/{dto.TaskId}");
            return View(dto);
        }

        try
        {
            await _httpClient.PutAsJsonOrThrowAsync("api/housekeeping/status", dto);
            TempData["Success"] = "Task status updated! Room status synchronized.";
            return RedirectToAction(nameof(Index), new { role = staffRole });
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            ViewBag.Task = await _httpClient.GetFromJsonOrThrowAsync<HousekeepingTaskResponseDTO>($"api/housekeeping/{dto.TaskId}");
            return View(dto);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        string? staffRole = HttpContext.Session.GetString("StaffRole");
        if (string.IsNullOrEmpty(staffRole) || (staffRole != "Housekeeping" && staffRole != "Manager" && staffRole != "FrontDesk"))
        {
            if (string.IsNullOrEmpty(staffRole)) return RedirectToAction("Login", "Staff");
            return RedirectToAction("Dashboard", "Staff", new { role = staffRole });
        }

        ViewData["Role"] = staffRole;
        try
        {
            await _httpClient.DeleteOrThrowAsync($"api/housekeeping/{id}");
            TempData["Success"] = "Housekeeping task deleted.";
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
        }
        return RedirectToAction(nameof(Index), new { role = staffRole });
    }
}
