using BookIt.API.DTOs;
using BookIt.API.Models;
using BookIt.API.Repositories.Interfaces;
using BookIt.API.Services.Interfaces;

namespace BookIt.API.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IFileStorageService _fileStorageService;

    public UserService(IUserRepository userRepository, IFileStorageService fileStorageService)
    {
        _userRepository = userRepository;
        _fileStorageService = fileStorageService;
    }

    public async Task<IEnumerable<UserDto>> GetAllAsync()
    {
        var users = await _userRepository.GetAllAsync();
        return users.Select(MapToDto);
    }

    public async Task<UserDto?> GetByIdAsync(Guid id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        return user == null ? null : MapToDto(user);
    }

    public async Task ChangePasswordAsync(Guid userId, ChangePasswordDto dto)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            throw new KeyNotFoundException("El usuario no existe.");

        if (!BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, user.PasswordHash))
            throw new UnauthorizedAccessException("La contraseña actual es incorrecta.");

        if (dto.CurrentPassword == dto.NewPassword)
            throw new ArgumentException("La nueva contraseña debe ser distinta de la actual.");

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword, workFactor: 12);
        user.FechaActualizacion = DateTime.UtcNow;

        await _userRepository.UpdateAsync(user);
    }

    public async Task<UserDto> UpdateProfileImageAsync(Guid userId, UpdateProfileImageDto dto)
    {
        if (dto.ProfileImage == null || dto.ProfileImage.Length == 0)
            throw new ArgumentException("Seleccioná una imagen de perfil.");

        var user = await _userRepository.GetByIdAsync(userId)
            ?? throw new KeyNotFoundException("El usuario no existe.");

        var previousProfileImage = user.ProfileImageUrl;
        var newProfileImageUrl = await _fileStorageService.SaveSingleAsync(dto.ProfileImage, "users", $"user-{user.Id}");

        if (!string.IsNullOrWhiteSpace(previousProfileImage))
        {
            _fileStorageService.DeleteByUrl(previousProfileImage);
        }

        user.ProfileImageUrl = newProfileImageUrl;
        user.FechaActualizacion = DateTime.UtcNow;

        await _userRepository.UpdateAsync(user);
        return MapToDto(user);
    }

    private static UserDto MapToDto(User user) => new()
    {
        Id = user.Id,
        Nombre = user.Nombre,
        Telefono = user.Telefono,
        Email = user.Email,
        ProfileImageUrl = user.ProfileImageUrl,
        Rol = user.Rol,
        Activo = user.Activo,
        FechaCreacion = user.FechaCreacion,
        FechaActualizacion = user.FechaActualizacion
    };
}

