using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace CogStayMVC.Controllers;

public static class ControllerExtensions
{
    public static void AttachBearerToken(this HttpClient client, HttpContext httpContext)
    {
        var token = httpContext.Session.GetString("JwtToken");
        if (!string.IsNullOrEmpty(token))
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
    }

    public static async Task<T> GetFromJsonOrThrowAsync<T>(this HttpClient client, string requestUri, HttpContext? httpContext = null)
    {
        if (httpContext != null) client.AttachBearerToken(httpContext);
        var response = await client.GetAsync(requestUri);
        return await HandleResponseAsync<T>(response);
    }

    public static async Task<T> PostAsJsonOrThrowAsync<T, TValue>(this HttpClient client, string requestUri, TValue value, HttpContext? httpContext = null)
    {
        if (httpContext != null) client.AttachBearerToken(httpContext);
        var response = await client.PostAsJsonAsync(requestUri, value);
        return await HandleResponseAsync<T>(response);
    }

    public static async Task PostAsJsonOrThrowAsync<TValue>(this HttpClient client, string requestUri, TValue value, HttpContext? httpContext = null)
    {
        if (httpContext != null) client.AttachBearerToken(httpContext);
        var response = await client.PostAsJsonAsync(requestUri, value);
        await HandleResponseAsync(response);
    }

    public static async Task PutAsJsonOrThrowAsync<TValue>(this HttpClient client, string requestUri, TValue value, HttpContext? httpContext = null)
    {
        if (httpContext != null) client.AttachBearerToken(httpContext);
        var response = await client.PutAsJsonAsync(requestUri, value);
        await HandleResponseAsync(response);
    }

    public static async Task PatchAsJsonOrThrowAsync<TValue>(this HttpClient client, string requestUri, TValue value, HttpContext? httpContext = null)
    {
        if (httpContext != null) client.AttachBearerToken(httpContext);
        var response = await client.PatchAsJsonAsync(requestUri, value);
        await HandleResponseAsync(response);
    }

    public static async Task DeleteOrThrowAsync(this HttpClient client, string requestUri, HttpContext? httpContext = null)
    {
        if (httpContext != null) client.AttachBearerToken(httpContext);
        var response = await client.DeleteAsync(requestUri);
        await HandleResponseAsync(response);
    }

    private static async Task<T> HandleResponseAsync<T>(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<T>();
            if (result == null)
            {
                throw new Exception("Received empty response from the API.");
            }
            return result;
        }

        var errorMsg = await ExtractErrorMessageAsync(response);
        throw new Exception(errorMsg);
    }

    private static async Task HandleResponseAsync(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var errorMsg = await ExtractErrorMessageAsync(response);
        throw new Exception(errorMsg);
    }

    private static async Task<string> ExtractErrorMessageAsync(HttpResponseMessage response)
    {
        try
        {
            var content = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(content))
            {
                return $"API returned status code {(int)response.StatusCode}: {response.ReasonPhrase}";
            }

            using var doc = JsonDocument.Parse(content);
            var root = doc.RootElement;
            if (root.ValueKind == JsonValueKind.Object)
            {
                if (root.TryGetProperty("message", out var msgProp))
                {
                    return msgProp.GetString() ?? "An error occurred.";
                }

                if (root.TryGetProperty("errors", out var errorsProp))
                {
                    return errorsProp.ToString();
                }
            }
            return content;
        }
        catch
        {
            return $"API returned status code {(int)response.StatusCode}: {response.ReasonPhrase}";
        }
    }
}
