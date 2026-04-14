using UniConnect.API.DTOs;

namespace UniConnect.API.Services;

public interface IUserService
{
    Task<UserDto?> GetUserByIdAsync(int userId);
    Task<UserDto?> UpdateUserProfileAsync(int userId, UserDto dto);
    Task<int> GetUserPointsAsync(int userId);
}
