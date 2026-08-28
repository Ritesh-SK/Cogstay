using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using CogStay.Application.DTOs;

namespace CogStayMVC.Controllers;

public class HomeController : Controller
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<HomeController> _logger;

    public HomeController(IHttpClientFactory httpClientFactory, ILogger<HomeController> logger)
    {
        _httpClient = httpClientFactory.CreateClient("CogStayApi");
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        try
        {
            var availableRooms = await _httpClient.GetFromJsonOrThrowAsync<IEnumerable<RoomResponseDTO>>("api/rooms/available", HttpContext);
            return View(availableRooms);
        }
        catch (System.Exception ex)
        {
            _logger.LogWarning(ex, "[HomeController.Index Warning] Unable to fetch available rooms directly from API.");
            return View(new List<RoomResponseDTO>());
        }
    }

    public IActionResult Error()
    {
        var requestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        ViewData["RequestId"] = requestId;

        var exceptionHandlerFeature = HttpContext.Features.Get<IExceptionHandlerFeature>();
        if (exceptionHandlerFeature != null)
        {
            _logger.LogError(exceptionHandlerFeature.Error, 
                "[Production Error Captured] RequestId: {RequestId} | Path: {Path} | Message: {Message}",
                requestId,
                exceptionHandlerFeature.Path,
                exceptionHandlerFeature.Error.Message);
        }

        return View();
    }
}
