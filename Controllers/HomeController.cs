using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using CogStayMVC.Controllers.Api;

namespace CogStayMVC.Controllers;

public class HomeController : Controller
{
    private readonly RoomApiController _roomApiController;

    public HomeController(RoomApiController roomApiController)
    {
        _roomApiController = roomApiController;
    }

    public async Task<IActionResult> Index()
    {
        var availableRooms = ControllerExtensions.Unpack(await _roomApiController.GetAvailableRooms());
        return View(availableRooms);
    }

    public IActionResult Error()
    {
        ViewData["RequestId"] = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        return View();
    }
}
