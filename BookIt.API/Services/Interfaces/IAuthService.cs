using BookIt.API.DTOs;

namespace BookIt.API.Services.Interfaces;

public interface IAuthService
{
    Task<AuthResponseDto> RegisterAsync(RegisterDto dto);
    Task<AuthResponseDto> RegisterVendorAsync(RegisterVendorDto dto);
    Task<AuthResponseDto> LoginAsync(LoginDto dto);
}
