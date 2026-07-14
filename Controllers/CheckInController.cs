using Microsoft.AspNetCore.Mvc;

namespace CogStayMVC.Controllers;

public class CheckInController : Controller
{
    public IActionResult CheckIn() => View();
    public IActionResult CheckOut() => View();
    public IActionResult ActiveStays() => View();
}
