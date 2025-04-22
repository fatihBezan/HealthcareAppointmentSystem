using Healthcare.Application.DTOs;

namespace Healthcare.Application.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponseDto> RegisterAsync(RegisterUserDto registerDto);
        Task<AuthResponseDto> LoginAsync(LoginUserDto loginDto);
        Task<UserDto> GetCurrentUserAsync(int userId);
        Task<bool> IsInRoleAsync(int userId, string roleName);
    }
} 