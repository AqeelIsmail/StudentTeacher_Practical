using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using StudentTeacherPractical.Mvc.Models;

namespace StudentTeacherPractical.Mvc.Controllers;

public class AccountController : Controller
{
    private readonly IHttpClientFactory _clientFactory;

    public AccountController(IHttpClientFactory clientFactory)
    {
        _clientFactory = clientFactory;
    }

    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        // Check the information entered in the form.
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        // Get the HttpClient configured in Program.cs.
        var client = _clientFactory.CreateClient("StudentTeacherApi");

        // Convert the form information into JSON.
        var json = JsonConvert.SerializeObject(model);

        var content = new StringContent(
            json, Encoding.UTF8, "application/json");

        // Send the information to the API registration endpoint.
        var response = await client.PostAsync(
            "api/auth/register", content);

        // Check whether the API successfully registered the user.
        if (response.IsSuccessStatusCode)
        {
            return RedirectToAction("Login");
        }

        ModelState.AddModelError(
            "", $"Registration failed. API returned {(int)response.StatusCode}.");

        return View(model);
    }

    //-------------------------------------------------------------------------
    //login
    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        // Create the API client.
        var client = _clientFactory.CreateClient("StudentTeacherApi");

        // Convert the entered login details into JSON.
        var json = JsonConvert.SerializeObject(model);
        var content = new StringContent(
            json, Encoding.UTF8, "application/json");

        // Send the login details to the API.
        var response = await client.PostAsync("api/auth/login", content);

        if (!response.IsSuccessStatusCode)
        {
            ModelState.AddModelError("", "Invalid email or password.");
            return View(model);
        }

        // Read the authentication cookie returned by the API.
        if (!response.Headers.TryGetValues("Set-Cookie", out var headers))
        {
            ModelState.AddModelError("", "No authentication cookie returned.");
            return View(model);
        }

        var setCookie = headers.FirstOrDefault(
            c => c.StartsWith(".AspNetCore.Cookies=", StringComparison.Ordinal));

        if (setCookie == null)
        {
            ModelState.AddModelError("", "Authentication cookie not found.");
            return View(model);
        }

        // Keep only the cookie name and value.
        var authCookie = setCookie.Split(';', 2)[0];

        // Ask the API which user and role this cookie belongs to.
        using var profileRequest = new HttpRequestMessage(
            HttpMethod.Get, "api/auth/profile");

        profileRequest.Headers.TryAddWithoutValidation(
            "Cookie", authCookie);

        using var profileResponse = await client.SendAsync(profileRequest);

        if (!profileResponse.IsSuccessStatusCode)
        {
            ModelState.AddModelError("", "Unable to verify the account.");
            return View(model);
        }

        using var document = JsonDocument.Parse(
            await profileResponse.Content.ReadAsStringAsync());

        int userId = document.RootElement
            .GetProperty("userId").GetInt32();

        string? role = document.RootElement
            .GetProperty("role").GetString();

        if (role != "Student" && role != "Teacher")
        {
            ModelState.AddModelError("", "Invalid account role.");
            return View(model);
        }

        // Save the verified authentication details in MVC session.
        HttpContext.Session.SetString("ApiAuthCookie", authCookie);
        HttpContext.Session.SetString("ApiRole", role);
        HttpContext.Session.SetInt32("ApiUserId", userId);

        // return to the existing home page.
        return RedirectToAction("Index", "Home");
    }

    //-------------------------------------------------------------------------
    //logout
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        // Get the API cookie that MVC saved during login.
        var authCookie = HttpContext.Session.GetString("ApiAuthCookie");

        if (!string.IsNullOrEmpty(authCookie))
        {
            var client = _clientFactory.CreateClient("StudentTeacherApi");

            // Create a POST request to the API logout endpoint.
            using var request = new HttpRequestMessage(
                HttpMethod.Post,
                "api/auth/logout");

            // Attach the current user's API cookie.
            request.Headers.TryAddWithoutValidation(
                "Cookie", authCookie);

            try
            {
                using var response = await client.SendAsync(request);
            }
            catch (HttpRequestException)
            {
                // Even if the API is unavailable, clear MVC's session.
            }
        }

        // Remove the information stored during login.
        HttpContext.Session.Remove("ApiAuthCookie");
        HttpContext.Session.Remove("ApiRole");
        HttpContext.Session.Remove("ApiUserId");

        return RedirectToAction("Login");
    }
}

