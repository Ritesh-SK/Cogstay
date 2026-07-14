using Microsoft.AspNetCore.Mvc;

namespace CogStayMVC.Controllers;

public class ReservationController : Controller
{
    public IActionResult Index() => View();
    public IActionResult Create() => View();
    public IActionResult Details() => View();
}
