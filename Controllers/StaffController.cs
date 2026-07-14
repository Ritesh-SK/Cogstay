using Microsoft.AspNetCore.Mvc;

namespace CogStayMVC.Controllers;

public class StaffController : Controller
{
    public IActionResult Login() => View();

    public IActionResult Dashboard(string role = "Admin")
    {
        ViewData["Role"] = role;
        return View();
    }

    public IActionResult Index() => View();
    public IActionResult Create() => View();
    public IActionResult Edit() => View();
    public IActionResult Details() => View();
    public IActionResult Delete() => View();
}
