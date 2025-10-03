using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Trackademy.Application.authenticator.Models;
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
    [HttpPost("create")]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
    {
        if (!ValidateData(request))
        {
            return Conflict("Не все поля заполнены.");
        }

        if (!VerifyNullEmailAndNicknameAndPassword(request.Email, request.Password))
        {
            return BadRequest("Email и пароль обязательны");
        }

        var organization = await db.Organizations
            .Where(x => x.Id == request.OrganizationId)
            .FirstOrDefaultAsync();

        if (organization == null)
        {
            return BadRequest("Ошибка с организацией");
        }

        var exists = await db.Users
            .Where(x => x.OrganizationId == request.OrganizationId)
            .AnyAsync(u => u.Login == request.Login);

        if (exists)
        {
            return Conflict("Пользователь с таким login уже существует");
        }

        var user = new User
        {
            Login = request.Login,
            FullName = request.FullName,
            Email = request.Email,
            Phone = request.Phone,
            ParentPhone = request.ParentPhone,
            Role = request.Role,
            CreatedDate = DateTime.UtcNow,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            OrganizationId = request.OrganizationId,
            Organization = organization
        };

        await db.Users.AddAsync(user);
        await db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetMe), new { id = user.Id }, new
        {
            user.Id,
            user.FullName,
            user.Login,
            user.Email,
            Role = user.Role.ToString()
        });
    }

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


    private bool ValidateData(CreateUserRequest request)
    {
        if (request == null) return false;
        if (string.IsNullOrWhiteSpace(request.FullName)) return false;
        if (string.IsNullOrWhiteSpace(request.Phone)) return false;
        if (string.IsNullOrWhiteSpace(request.Password)) return false;
        if (request.Role == default) return false;

        return true;
    }

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

    private bool VerifyNullEmailAndNicknameAndPassword(string? email, string? password)
    {
        if (string.IsNullOrWhiteSpace(email) && string.IsNullOrWhiteSpace(password))
        {
            
            return false;
        }

        return true;
    }

    #endregion
}