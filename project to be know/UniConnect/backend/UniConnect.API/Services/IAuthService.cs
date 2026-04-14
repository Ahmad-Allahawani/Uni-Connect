using UniConnect.API.DTOs;

namespace UniConnect.API.Services;

public interface IAuthService
{
    Task<AuthResponseDto> RegisterAsync(RegisterDto dto);
    Task<AuthResponseDto> LoginAsync(LoginDto dto);
    string GenerateJwtToken(int userId);
}
