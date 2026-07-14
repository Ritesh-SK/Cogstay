using Microsoft.AspNetCore.Mvc;

namespace CogStayMVC.Controllers;

public class GuestController : Controller
{
    public IActionResult Login() => View();
    public IActionResult Register() => View();
    public IActionResult Dashboard() => View();
    public IActionResult AvailableRooms() => View();
    public IActionResult BookRoom() => View();
    public IActionResult MyReservations() => View();
    public IActionResult BookingHistory() => View();
    public IActionResult CheckInStatus() => View();
    public IActionResult Billing() => View();
    public IActionResult Profile() => View();
}
