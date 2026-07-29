using Microsoft.AspNetCore.Mvc;
using System;

namespace CogStayMVC.Controllers;

public static class ControllerExtensions
{
    public static T Unpack<T>(ActionResult<T> actionResult)
    {
        if (actionResult.Value != null)
        {
            return actionResult.Value;
        }
        
        if (actionResult.Result is OkObjectResult okResult && okResult.Value is T val)
        {
            return val;
        }
        
        if (actionResult.Result is CreatedAtActionResult createdResult && createdResult.Value is T val2)
        {
            return val2;
        }
        
        if (actionResult.Result is ObjectResult objectResult)
        {
            var errorMsg = ExtractErrorMessage(objectResult.Value);
            throw new Exception(errorMsg);
        }
        
        throw new Exception("An unexpected error occurred while calling the API.");
    }

    public static void Unpack(IActionResult actionResult)
    {
        if (actionResult is NoContentResult)
        {
            return;
        }
        
        if (actionResult is ObjectResult objectResult)
        {
            var errorMsg = ExtractErrorMessage(objectResult.Value);
            throw new Exception(errorMsg);
        }
        
        throw new Exception("An unexpected error occurred while calling the API.");
    }

    private static string ExtractErrorMessage(object? value)
    {
        if (value == null) return "An error occurred.";
        
        var prop = value.GetType().GetProperty("message");
        if (prop != null)
        {
            return prop.GetValue(value)?.ToString() ?? "An error occurred.";
        }
        
        if (value is string str) return str;
        
        return value.ToString() ?? "An error occurred.";
    }
}
