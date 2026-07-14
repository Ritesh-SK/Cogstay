using Microsoft.AspNetCore.Mvc;

namespace CogStayMVC.Controllers;

public class HousekeepingController : Controller
{
    public IActionResult Index() => View();
    public IActionResult Create() => View();
    public IActionResult Edit() => View();
    public IActionResult Details() => View();
}
