using UniConnect.API.Data;
using UniConnect.API.DTOs;
using UniConnect.API.Models;
using Microsoft.EntityFrameworkCore;

namespace UniConnect.API.Services;

public class UserService : IUserService
{
    private readonly UniConnectContext _context;

    public UserService(UniConnectContext context)
    {
        _context = context;
    }

    public async Task<UserDto?> GetUserByIdAsync(int userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null) return null;

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

    public async Task<UserDto?> UpdateUserProfileAsync(int userId, UserDto dto)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null) return null;

        user.FirstName = dto.FirstName;
        user.LastName = dto.LastName;
        user.Email = dto.Email;
        user.UpdatedAt = DateTime.UtcNow;

        _context.Users.Update(user);
        await _context.SaveChangesAsync();

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

    public async Task<int> GetUserPointsAsync(int userId)
    {
        var user = await _context.Users.FindAsync(userId);
        return user?.Points ?? 0;
    }
}
