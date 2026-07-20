using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using CogStayMVC.DTOs;
using CogStayMVC.Services.Interfaces;

namespace CogStayMVC.Controllers;

public class HousekeepingController : Controller
{
    private readonly IHousekeepingService _housekeepingService;
    private readonly IRoomService _roomService;

    public HousekeepingController(
        IHousekeepingService housekeepingService,
        IRoomService roomService)
    {
        _housekeepingService = housekeepingService;
        _roomService = roomService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var tasks = await _housekeepingService.GetAllTasksAsync();
        return View(tasks);
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var task = await _housekeepingService.GetTaskByIdAsync(id);
        if (task == null) return NotFound();
        return View(task);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        ViewBag.Rooms = await _roomService.GetAllRoomsAsync();
        return View(new CreateHousekeepingTaskDTO());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateHousekeepingTaskDTO dto)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Rooms = await _roomService.GetAllRoomsAsync();
            return View(dto);
        }

        try
        {
            await _housekeepingService.CreateTaskAsync(dto);
            TempData["Success"] = "Housekeeping cleaning request created.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            ViewBag.Rooms = await _roomService.GetAllRoomsAsync();
            return View(dto);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var task = await _housekeepingService.GetTaskByIdAsync(id);
        if (task == null) return NotFound();

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
        if (id != dto.TaskId) return BadRequest();

        if (!ModelState.IsValid)
        {
            ViewBag.Task = await _housekeepingService.GetTaskByIdAsync(dto.TaskId);
            return View(dto);
        }

        try
        {
            await _housekeepingService.UpdateTaskStatusAsync(dto);
            TempData["Success"] = "Task status updated! Room status synchronized.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            ViewBag.Task = await _housekeepingService.GetTaskByIdAsync(dto.TaskId);
            return View(dto);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await _housekeepingService.DeleteTaskAsync(id);
            TempData["Success"] = "Housekeeping task deleted.";
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
        }
        return RedirectToAction(nameof(Index));
    }
}
