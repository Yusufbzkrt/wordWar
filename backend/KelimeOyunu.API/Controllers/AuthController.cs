using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using KelimeOyunu.Core.DTOs;
using KelimeOyunu.Core.Entities;
using KelimeOyunu.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace KelimeOyunu.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _config;
    private readonly KelimeOyunu.Core.Interfaces.IQuestManager _questManager;

    public AuthController(AppDbContext context, IConfiguration config, KelimeOyunu.Core.Interfaces.IQuestManager questManager)
    {
        _context = context;
        _config = config;
        _questManager = questManager;
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponseDto>> Register(RegisterRequestDto dto)
    {
        if (await _context.Users.AnyAsync(u => u.Username == dto.Username))
            return BadRequest("Bu kullanıcı adı zaten kullanılıyor.");

        var user = new User
        {
            Username = dto.Username,
            PasswordHash = HashPassword(dto.Password)
        };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var token = GenerateToken(user);
        return Ok(new AuthResponseDto(token, new UserProfileDto(user.Id, user.Username, user.Gold, user.Diamonds, 0, 0, null)));
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login(LoginRequestDto dto)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == dto.Username);
        if (user == null || !VerifyPassword(dto.Password, user.PasswordHash))
            return Unauthorized("Geçersiz kullanıcı adı veya şifre.");

        var token = GenerateToken(user);
        
        // Günlük görevleri ata ve Login eventini tetikle
        await _questManager.GetOrAssignDailyQuestsAsync(user.Id);
        await _questManager.TrackEventAsync(user.Id, KelimeOyunu.Core.Enums.QuestEventType.Login);

        return Ok(new AuthResponseDto(token, new UserProfileDto(user.Id, user.Username, user.Gold, user.Diamonds, user.TotalWins, user.TotalLosses, user.LastAdRewardTime)));
    }

    private string GenerateToken(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
        var claims = new[] {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username)
        };
        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddDays(7),
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static string HashPassword(string password)
    {
        using var sha = SHA256.Create();
        return Convert.ToBase64String(sha.ComputeHash(Encoding.UTF8.GetBytes(password)));
    }

    private static bool VerifyPassword(string password, string hash)
    {
        return HashPassword(password) == hash;
    }
}
