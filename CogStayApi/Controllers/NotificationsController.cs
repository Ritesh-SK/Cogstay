using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CogStay.Application.Contracts.Services;
using CogStay.Application.DTOs;

namespace CogStayApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _notificationService;

    public NotificationsController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    [HttpGet]
    public async Task<IActionResult> GetMyNotifications([FromQuery] int limit = 10)
    {
        string userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("IntegerId") ?? "All";
        string role = User.FindFirstValue(ClaimTypes.Role) ?? "Guest";

        var result = await _notificationService.GetUserNotificationsAsync(userId, role, limit);
        return Ok(result);
    }

    [HttpGet("unread-count")]
    public async Task<IActionResult> GetUnreadCount()
    {
        string userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("IntegerId") ?? "All";
        string role = User.FindFirstValue(ClaimTypes.Role) ?? "Guest";

        var result = await _notificationService.GetUnreadCountAsync(userId, role);
        return Ok(result);
    }

    [HttpPost("{id}/read")]
    public async Task<IActionResult> MarkAsRead(string id)
    {
        await _notificationService.MarkAsReadAsync(id);
        return Ok(new { Message = "Notification marked as read." });
    }

    [HttpPost("read-all")]
    public async Task<IActionResult> MarkAllAsRead()
    {
        string userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("IntegerId") ?? "All";
        string role = User.FindFirstValue(ClaimTypes.Role) ?? "Guest";

        await _notificationService.MarkAllAsReadAsync(userId, role);
        return Ok(new { Message = "All notifications marked as read." });
    }

    [HttpPost]
    public async Task<IActionResult> CreateNotification([FromBody] CreateNotificationDTO dto)
    {
        await _notificationService.CreateNotificationAsync(dto);
        return Ok(new { Message = "Notification created successfully." });
    }
}
