using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using CogStayMVC.Services.Interfaces;

namespace CogStayMVC.Controllers;

public class HomeController : Controller
{
    private readonly IRoomService _roomService;

    public HomeController(IRoomService roomService)
    {
        _roomService = roomService;
    }

    public async Task<IActionResult> Index()
    {
        var availableRooms = await _roomService.GetAvailableRoomsAsync();
        return View(availableRooms);
    }

    public IActionResult Error()
    {
        ViewData["RequestId"] = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        return View();
    }
}
