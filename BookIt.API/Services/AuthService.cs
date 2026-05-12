using System.IdentityModel.Tokens.Jwt;
using System.Net.Mail;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;
using BookIt.API.DTOs;
using BookIt.API.Models;
using BookIt.API.Repositories.Interfaces;
using BookIt.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace BookIt.API.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IServiceRepository _serviceRepository;
    private readonly IConfiguration _configuration;

    public AuthService(IUserRepository userRepository, IServiceRepository serviceRepository, IConfiguration configuration)
    {
        _userRepository = userRepository;
        _serviceRepository = serviceRepository;
        _configuration = configuration;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
    {
        // Normalize email for consistency
        var normalizedEmail = dto.Email.ToLower().Trim();

        if (!IsStrictEmailValid(normalizedEmail))
            throw new ArgumentException("El email debe incluir un dominio válido (por ejemplo, usuario@dominio.com).");

        // Validate email uniqueness first
        if (await _userRepository.ExistsByEmailAsync(normalizedEmail))
            throw new InvalidOperationException("El email ya está registrado.");

        // Validate role is allowed (prevent privilege escalation)
        var allowedRoles = new[] { "usuario", "vendedor", "operador" };
        if (!allowedRoles.Contains(dto.Rol.ToLower()))
            throw new ArgumentException("El rol especificado no es válido.");

        var user = new User
        {
            Nombre = dto.Nombre.Trim(),
            Telefono = dto.Telefono.Trim(),
            Email = normalizedEmail,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password, workFactor: 12), // Increase work factor for better security
            Rol = dto.Rol.ToLower(),
            FechaCreacion = DateTime.UtcNow,
            FechaActualizacion = DateTime.UtcNow
        };

        // Generate token BEFORE saving to DB to ensure security
        // If token generation fails, no user will be created
        var token = GenerateJwtToken(user);

        try
        {
            // Only save to DB after token is successfully generated
            await _userRepository.CreateAsync(user);
        }
        catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("IX_Users_Email") ?? false)
        {
            // Handle race condition: email already registered by concurrent request
            throw new InvalidOperationException("El email ya está registrado.");
        }

        return new AuthResponseDto
        {
            Token = token,
            User = MapToDto(user)
        };
    }

    public async Task<AuthResponseDto> RegisterVendorAsync(RegisterVendorDto dto)
    {
        // Normalize email for consistency
        var normalizedEmail = dto.Email.ToLower().Trim();

        if (!IsStrictEmailValid(normalizedEmail))
            throw new ArgumentException("El email debe incluir un dominio válido (por ejemplo, usuario@dominio.com).");

        // Validate email uniqueness first
        if (await _userRepository.ExistsByEmailAsync(normalizedEmail))
            throw new InvalidOperationException("El email ya está registrado.");

        // Validate price range
        if (dto.PrecioMinimo > dto.PrecioMaximo)
            throw new ArgumentException("El precio mínimo no puede ser mayor al precio máximo.");

        var tipoServicio = dto.TipoServicio.Trim();
        var isSalon = string.Equals(tipoServicio, "Salón", StringComparison.OrdinalIgnoreCase)
            || string.Equals(tipoServicio, "Salon", StringComparison.OrdinalIgnoreCase);

        if (!isSalon && (dto.CategoryIds == null || dto.CategoryIds.Count == 0))
            throw new ArgumentException("Los servicios que no son Salón deben tener al menos una categoría de evento.");

        var user = new User
        {
            Nombre = dto.Nombre.Trim(),
            Telefono = dto.Telefono.Trim(),
            Email = normalizedEmail,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password, workFactor: 12),
            Rol = "vendedor",
            FechaCreacion = DateTime.UtcNow,
            FechaActualizacion = DateTime.UtcNow
        };

        var service = new Service
        {
            VendorId = user.Id,
            Nombre = dto.NombreServicio.Trim(),
            Descripcion = dto.DescripcionServicio.Trim(),
            Ubicacion = dto.Ubicacion.Trim(),
            TipoServicio = tipoServicio,
            PrecioMinimo = dto.PrecioMinimo,
            PrecioMaximo = dto.PrecioMaximo,
            // Guardar campos opcionales si existen
            Direccion = !string.IsNullOrWhiteSpace(dto.Direccion) ? dto.Direccion.Trim() : null,
            Capacidad = dto.Capacidad,
            FechaCreacion = DateTime.UtcNow,
            FechaActualizacion = DateTime.UtcNow,
            ServiceEventCategories = (dto.CategoryIds ?? new List<Guid>())
                .Select(categoryId => new ServiceEventCategory
                {
                    EventCategoryId = categoryId
                })
                .ToList()
        };

        // Generate token BEFORE saving to DB to ensure security
        var token = GenerateJwtToken(user);

        try
        {
            // Save user first (this generates the user.Id)
            await _userRepository.CreateAsync(user);
            // Then save service with the vendorId
            service.VendorId = user.Id;
            await _serviceRepository.CreateAsync(service);
        }
        catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("IX_Users_Email") ?? false)
        {
            throw new InvalidOperationException("El email ya está registrado.");
        }
        catch (DbUpdateException)
        {
            throw new InvalidOperationException("Error al crear el servicio. Por favor intenta de nuevo.");
        }

        return new AuthResponseDto
        {
            Token = token,
            User = MapToDto(user)
        };
    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
    {
        var user = await _userRepository.GetByEmailAsync(dto.Email);
        
        // Always verify password even if user doesn't exist (prevents timing attacks)
        var passwordValid = user != null && BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash);
        
        if (!passwordValid)
            throw new UnauthorizedAccessException("Credenciales inválidas.");

        if (!user!.Activo)
            throw new UnauthorizedAccessException("La cuenta se encuentra deshabilitada.");

        var token = GenerateJwtToken(user);
        return new AuthResponseDto
        {
            Token = token,
            User = MapToDto(user)
        };
    }

    private string GenerateJwtToken(User user)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey no configurada.");
        var issuer = jwtSettings["Issuer"] ?? throw new InvalidOperationException("JWT Issuer no configurado.");
        var audience = jwtSettings["Audience"] ?? throw new InvalidOperationException("JWT Audience no configurado.");
        var expiresInMinutes = int.Parse(jwtSettings["ExpiresInMinutes"] ?? "60");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Rol),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiresInMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static UserDto MapToDto(User user) => new()
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

    private static bool IsStrictEmailValid(string email)
    {
        try
        {
            var normalized = new MailAddress(email).Address;
            if (!string.Equals(normalized, email, StringComparison.OrdinalIgnoreCase))
                return false;

            return Regex.IsMatch(normalized, @"^[^@\s]+@[^@\s]+\.[^@\s]{2,}$", RegexOptions.CultureInvariant);
        }
        catch
        {
            return false;
        }
    }
}
