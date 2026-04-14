using BC = BCrypt.Net.BCrypt;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using UniConnect.API.Data;
using UniConnect.API.DTOs;
using UniConnect.API.Models;
using Microsoft.EntityFrameworkCore;

namespace UniConnect.API.Services;

public class AuthService : IAuthService
{
    private readonly UniConnectContext _context;
    private readonly IConfiguration _configuration;

    public AuthService(UniConnectContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
    {
        if (await _context.Users.AnyAsync(u => u.UniversityId == dto.UniversityId))
            return new AuthResponseDto { Success = false, Message = "University ID already exists" };

        if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
            return new AuthResponseDto { Success = false, Message = "Email already exists" };

        var user = new User
        {
            UniversityId = dto.UniversityId,
            Email = dto.Email,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            PasswordHash = BC.HashPassword(dto.Password),
            Points = 100
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var token = GenerateJwtToken(user.Id);
        return new AuthResponseDto
        {
            Success = true,
            Message = "Registration successful",
            Token = token,
            User = MapToUserDto(user)
        };
    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.UniversityId == dto.UniversityId);
        if (user == null || !BC.Verify(dto.Password, user.PasswordHash))
            return new AuthResponseDto { Success = false, Message = "Invalid credentials" };

        if (user.IsBanned)
            return new AuthResponseDto { Success = false, Message = "User account is banned" };

        var token = GenerateJwtToken(user.Id);
        return new AuthResponseDto
        {
            Success = true,
            Message = "Login successful",
            Token = token,
            User = MapToUserDto(user)
        };
    }

    public string GenerateJwtToken(int userId)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var key = Encoding.ASCII.GetBytes(jwtSettings["SecretKey"] ?? "");
        var expirationMinutes = int.Parse(jwtSettings["ExpirationMinutes"] ?? "60");

        var tokenHandler = new JwtSecurityTokenHandler();
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[] { new Claim("userId", userId.ToString()) }),
            Expires = DateTime.UtcNow.AddMinutes(expirationMinutes),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    private UserDto MapToUserDto(User user)
    {
        return new UserDto
        {
            Id = user.Id,
            UniversityId = user.UniversityId,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Points = user.Points,
            IsVerified = user.IsVerified,
            IsAdmin = user.IsAdmin
        };
    }
}
