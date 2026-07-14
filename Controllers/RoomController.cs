using Microsoft.AspNetCore.Mvc;

namespace CogStayMVC.Controllers;

public class RoomController : Controller
{
    public IActionResult Index() => View();
    public IActionResult Create() => View();
    public IActionResult Edit() => View();
    public IActionResult Details() => View();
    public IActionResult CheckAvailability() => View();
}
