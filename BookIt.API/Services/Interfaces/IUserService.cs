using BookIt.API.DTOs;

namespace BookIt.API.Services.Interfaces;

public interface IUserService
{
    Task<IEnumerable<UserDto>> GetAllAsync();
    Task<UserDto?> GetByIdAsync(Guid id);
    Task ChangePasswordAsync(Guid userId, ChangePasswordDto dto);
    Task<UserDto> UpdateProfileImageAsync(Guid userId, UpdateProfileImageDto dto);
}
