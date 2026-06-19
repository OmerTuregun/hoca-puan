using HocaPuan.Core.DTOs.Auth;
using HocaPuan.Core.Interfaces.Services;

namespace HocaPuan.API.Tests;

internal sealed class FakeAuthService : IAuthService
{
    public Task<AuthResponseDto> RegisterAsync(RegisterDto dto) =>
        Task.FromResult(new AuthResponseDto { Success = false, Message = "test" });

    public Task<AuthResponseDto> LoginAsync(LoginDto dto) =>
        Task.FromResult(new AuthResponseDto { Success = false, Message = "test" });

    public Task<bool> VerifyEmailAsync(string token) => Task.FromResult(false);

    public Task<ForgotPasswordResponseDto> ForgotPasswordAsync(string email) =>
        Task.FromResult(new ForgotPasswordResponseDto { Message = "test" });

    public Task<bool> ResetPasswordAsync(ResetPasswordDto dto) => Task.FromResult(false);

    public Task<UserProfileDto?> GetProfileAsync(int userId) => Task.FromResult<UserProfileDto?>(null);
}
