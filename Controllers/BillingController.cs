using Microsoft.AspNetCore.Mvc;

namespace CogStayMVC.Controllers;

public class BillingController : Controller
{
    public IActionResult Index() => View();
    public IActionResult Create() => View();
    public IActionResult Payment() => View();
    public IActionResult History() => View();
}
