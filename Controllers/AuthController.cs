using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly ILogger<AuthController> _logger;

    public AuthController(ILogger<AuthController> logger)
    {
        _logger = logger;
    }
    
    [Authorize]
    [HttpGet("login")]
    public ActionResult Authorize()
    {
        return this.RedirectPermanent("/weatherforecast");
    }

    [HttpGet("/logout")]
    public async Task Unauthorize()
    {
        if (User.Identity != null && User.Identity.IsAuthenticated)
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignOutAsync(OpenIdConnectDefaults.AuthenticationScheme);
            return;
        }

        // Handle the case when the user is not authenticated
        HttpContext.Response.Redirect("/logout-google");
    }

    [HttpGet("/auth/me")]
    public ActionResult<UserModel> GetUserProfile()
    {
        var user = this.HttpContext.User;

        if (user?.Identity != null && user.Identity.IsAuthenticated)
        {
            return this.Ok(new UserModel
            {
                IsAuthenticated = true,
                FirstName = user.Claims?.FirstOrDefault(x => x.Type == "given_name")?.Value,
                LastName = user.Claims?
                    .FirstOrDefault(x => x.Type == "family_name")?.Value,
                EmailAddress = user.Claims?
                    .FirstOrDefault(x => x.Type == "email")?.Value,
            });
        }

        return this.Ok(new UserModel
        {
            IsAuthenticated = false,
        });
    }
}