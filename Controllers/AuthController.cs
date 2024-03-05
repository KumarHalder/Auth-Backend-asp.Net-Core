/*
  This is codebase is intended for 
  educational purpose, and strictly 
  not recommended for production use. 
*/

namespace Backend.Controllers;
using Model;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Security.Claims;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _configuration;

    public AuthController(IConfiguration configuration)
    {
        _configuration = configuration;
    }
    
    public static Dictionary<string, string> RefreshTokens = new Dictionary<string, string>();
    
    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginModel model)
    {
        // Check user credentials (in a real application, you'd authenticate against a database)
        if (model is { Username: "demo", Password: "password" })
        {
            var token = GenerateAccessToken(model.Username);
            // Generate refresh token
            var refreshToken = Guid.NewGuid().ToString();

            // Store the refresh token (in-memory for simplicity)
            RefreshTokens[refreshToken] = model.Username;

            return Ok(new { AccessToken = new JwtSecurityTokenHandler().WriteToken(token), RefreshToken = refreshToken });

        }
        return Unauthorized("Invalid credentials");
    }
    
    [HttpPost("refresh")]
    public IActionResult Refresh([FromBody] RefreshRequest request)
    {
        if (RefreshTokens.TryGetValue(request.RefreshToken, out var userId))
        {
            // Generate a new access token
            var token = GenerateAccessToken(userId);

            // Return the new access token to the client
            return Ok(new { AccessToken = new JwtSecurityTokenHandler().WriteToken(token) });
        }

        return BadRequest("Invalid refresh token");
    }

    private JwtSecurityToken GenerateAccessToken(string userName)
    {
        // Create user claims
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, userName),
            // Add additional claims as needed (e.g., roles, etc.)
        };

        // Create a JWT
        var token = new JwtSecurityToken(
            issuer: _configuration["JwtSettings:Issuer"],
            audience: _configuration["JwtSettings:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(1), // Token expiration time
            signingCredentials: new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:SecretKey"])),
                SecurityAlgorithms.HmacSha256)
        );

        return token;
    }
}