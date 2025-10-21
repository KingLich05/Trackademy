using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Trackademy.Application.Helper;
using Trackademy.Application.Persistance;
using Trackademy.Application.Users.Models;
using Trackademy.Domain.Users;

namespace Trackademy.Api.Controllers.Users;

[ApiController]
[Route("api/[controller]")]
public class AuthController(
    TrackademyDbContext db,
    IConfiguration config,
    ExtensionString str) : ControllerBase
{
    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetMe()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userId, out var id))
        {
            return Unauthorized();
        }

        var user = await db.Users
            .Include(user => user.Organization)
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user == null) return Unauthorized();

        return Ok(new
        {
            user.Id, 
            user.Login,
            user.FullName,
            user.Email,
            Role = user.Role,
            OrganizationId = user.Organization.Id,
            OrganizationName = user.Organization.Name
        });
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login(
        [FromBody] LogRequest request)
    {
        var user = await db.Users
            .Include(user => user.Organization)
            .FirstOrDefaultAsync(u => u.Login == request.Login);

        if (user == null || !VerifyPassword(request.Password, user.PasswordHash))
        {
            return Unauthorized(new { message = "Неверный Login или пароль" });
        }

        if (user.OrganizationId != request.OrganizationId)
        {
            return Unauthorized(new { message = "Неверная организация" });
        }

        var token = GenerateJwtToken(user);
        return Ok(new
        {
            token,
            user = new
            {
                user.Id,
                user.Login,
                user.FullName,
                user.Email,
                Role = user.Role.ToString(),
                OrganizationId = user.Organization.Id,
                OrganizationNames = user.Organization.Name
            }
        });
    }

    #region Private methods

    private string GenerateJwtToken(User user)
    {
        var jwtSection = config.GetSection("Jwt");

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, str.Str(user.Id)),
            new Claim(ClaimTypes.Email, user.Email ?? throw new InvalidOperationException()),
            new Claim(ClaimTypes.Role, str.Str(user.Role))
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSection["Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var expires = DateTime.UtcNow.AddHours(double.Parse(jwtSection["ExpiresHours"] ?? "8"));

        var token = new JwtSecurityToken(
            issuer: jwtSection["Issuer"],
            audience: jwtSection["Audience"],
            claims: claims,
            expires: expires,
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private bool VerifyPassword(string password, string storedHash)
    {
        return BCrypt.Net.BCrypt.Verify(password, storedHash);
    }

    #endregion
}