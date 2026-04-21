using BookIt.API.DTOs;
using BookIt.API.Repositories.Interfaces;
using BookIt.API.Services.Interfaces;

namespace BookIt.API.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;

    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<IEnumerable<UserDto>> GetAllAsync()
    {
        var users = await _userRepository.GetAllAsync();
        return users.Select(u => new UserDto
        {
            Id = u.Id,
            Nombre = u.Nombre,
            Telefono = u.Telefono,
            Email = u.Email,
            Rol = u.Rol,
            Activo = u.Activo,
            FechaCreacion = u.FechaCreacion,
            FechaActualizacion = u.FechaActualizacion
        });
    }

    public async Task<UserDto?> GetByIdAsync(Guid id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null) return null;

        return new UserDto
        {
            Id = user.Id,
            Nombre = user.Nombre,
            Telefono = user.Telefono,
            Email = user.Email,
            Rol = user.Rol,
            Activo = user.Activo,
            FechaCreacion = user.FechaCreacion,
            FechaActualizacion = user.FechaActualizacion
        };
    }
}
