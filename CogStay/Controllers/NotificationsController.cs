using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using CogStay.Application.DTOs;

namespace CogStayMVC.Controllers;

public class NotificationsController : Controller
{
    private readonly HttpClient _httpClient;

    public NotificationsController(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient("CogStayApi");
    }

    [HttpGet]
    public async Task<IActionResult> GetMyNotifications()
    {
        try
        {
            var notifications = await _httpClient.GetFromJsonOrThrowAsync<IEnumerable<NotificationResponseDTO>>("api/notifications", HttpContext);
            return Json(new { success = true, data = notifications });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message, data = new List<NotificationResponseDTO>() });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetUnreadCount()
    {
        try
        {
            var result = await _httpClient.GetFromJsonOrThrowAsync<UnreadCountDTO>("api/notifications/unread-count", HttpContext);
            return Json(new { success = true, unreadCount = result?.UnreadCount ?? 0 });
        }
        catch
        {
            return Json(new { success = false, unreadCount = 0 });
        }
    }

    [HttpPost]
    public async Task<IActionResult> MarkAsRead(string id)
    {
        try
        {
            await _httpClient.PostAsJsonOrThrowAsync<object, object>($"api/notifications/{id}/read", new { }, HttpContext);
            return Json(new { success = true });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    [HttpPost]
    public async Task<IActionResult> MarkAllAsRead()
    {
        try
        {
            await _httpClient.PostAsJsonOrThrowAsync<object, object>("api/notifications/read-all", new { }, HttpContext);
            return Json(new { success = true });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }
}
