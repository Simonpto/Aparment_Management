using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Apartment_Manager.Api.Controllers;

[ApiController]
[Route("api/admin")]
public class AdminController(IConfiguration config) : ControllerBase
{
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var adminPassword = config["Admin:Password"] ?? "admin";
        if (request.Password != adminPassword) return Unauthorized();

        var claims = new List<Claim> { new(ClaimTypes.Name, "admin"), new(ClaimTypes.Role, "Admin") };
        var identity  = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
        return Ok();
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return Ok();
    }

    [HttpGet("me"), Authorize]
    public IActionResult Me() => Ok(new { name = "admin" });
}

public record LoginRequest(string Password);
