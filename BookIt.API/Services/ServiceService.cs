using BookIt.API.DTOs;
using BookIt.API.Data;
using BookIt.API.Models;
using BookIt.API.Repositories.Interfaces;
using BookIt.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace BookIt.API.Services;

public class ServiceService : IServiceService
{
    private readonly IServiceRepository _serviceRepository;
    private readonly IUserRepository _userRepository;
    private readonly IFileStorageService _fileStorageService;
    private readonly ApplicationDbContext _context;

    public ServiceService(
        IServiceRepository serviceRepository,
        IUserRepository userRepository,
        IFileStorageService fileStorageService,
        ApplicationDbContext context)
    {
        _serviceRepository = serviceRepository;
        _userRepository = userRepository;
        _fileStorageService = fileStorageService;
        _context = context;
    }

    public async Task<IEnumerable<ServiceDto>> GetAllAsync()
    {
        var services = await _serviceRepository.GetAllAsync();
        return services.Select(MapToDto);
    }

    public async Task<IEnumerable<ServiceDto>> GetActiveAsync()
    {
        var services = await _serviceRepository.GetActiveAsync();
        return services.Select(MapToDto);
    }

    public async Task<ServiceDto?> GetByIdAsync(Guid id)
    {
        var service = await _serviceRepository.GetByIdAsync(id);
        return service == null ? null : MapToDto(service);
    }

    public async Task<IEnumerable<ServiceDto>> GetByVendorIdAsync(Guid vendorId)
    {
        var services = await _serviceRepository.GetByVendorIdAsync(vendorId);

        return services.Select(MapToDto);
    }

    public async Task<ServiceDto> CreateAsync(Guid currentUserId, CreateServiceDto dto)
    {
        var normalized = NormalizeAndValidate(dto);

        var user = await _userRepository.GetByIdAsync(currentUserId)
            ?? throw new KeyNotFoundException("El usuario no existe.");

        var service = new Service
        {
            VendorId = user.Id,
            Nombre = normalized.Nombre,
            Descripcion = normalized.Descripcion,
            Ubicacion = normalized.Ubicacion,
            TipoServicio = normalized.TipoServicio,
            PrecioMinimo = normalized.PrecioMinimo,
            PrecioMaximo = normalized.PrecioMaximo,
            Capacidad = normalized.Capacidad,
            DireccionCompleta = BuildDireccion(normalized),
            FechaCreacion = DateTime.UtcNow,
            FechaActualizacion = DateTime.UtcNow,
            ImageUrlsJson = await BuildImageUrlsJsonAsync(dto.Images, $"service-{Guid.NewGuid():N}"),
            ServiceEventCategories = (dto.CategoryIds ?? new List<Guid>())
                .Select(categoryId => new ServiceEventCategory
                {
                    EventCategoryId = categoryId
                })
                .ToList()
        };

        await using var transaction = await _context.Database.BeginTransactionAsync();

        if (!string.Equals(user.Rol, "vendedor", StringComparison.OrdinalIgnoreCase) && !string.Equals(user.Rol, "administrador", StringComparison.OrdinalIgnoreCase))
        {
            user.Rol = "vendedor";
            user.FechaActualizacion = DateTime.UtcNow;
            await _userRepository.UpdateAsync(user);
        }

        await _serviceRepository.CreateAsync(service);
        await transaction.CommitAsync();

        var createdService = await _serviceRepository.GetByIdAsync(service.Id)
            ?? throw new InvalidOperationException("No se pudo recuperar el servicio creado.");

        return MapToDto(createdService);
    }

    public async Task<ServiceDto> UpdateAsync(Guid serviceId, Guid currentUserId, bool isAdmin, CreateServiceDto dto)
    {
        var normalized = NormalizeAndValidate(dto);
        var service = await _serviceRepository.GetByIdAsync(serviceId)
            ?? throw new KeyNotFoundException("Servicio no encontrado.");

        if (!isAdmin && service.VendorId != currentUserId)
            throw new UnauthorizedAccessException("No tenés permiso para editar este servicio.");

        service.Nombre = normalized.Nombre;
        service.Descripcion = normalized.Descripcion;
        service.Ubicacion = normalized.Ubicacion;
        service.TipoServicio = normalized.TipoServicio;
        service.PrecioMinimo = normalized.PrecioMinimo;
        service.PrecioMaximo = normalized.PrecioMaximo;
        service.Capacidad = normalized.Capacidad;
        service.FechaActualizacion = DateTime.UtcNow;

        if (dto.Images != null && dto.Images.Count > 0)
        {
            _fileStorageService.DeleteMany(GetImageUrls(service.ImageUrlsJson));
            service.ImageUrlsJson = await BuildImageUrlsJsonAsync(dto.Images, $"service-{service.Id:N}");
        }

        if (service.DireccionCompleta == null)
        {
            service.DireccionCompleta = BuildDireccion(normalized);
        }
        else
        {
            service.DireccionCompleta.DepartamentoId = normalized.Direccion.DepartamentoId!.Value;
            service.DireccionCompleta.BarrioId = normalized.Direccion.BarrioId!.Value;
            service.DireccionCompleta.Calle = normalized.Direccion.Calle.Trim();
        }

        service.ServiceEventCategories.Clear();
        foreach (var categoryId in normalized.CategoryIds)
        {
            service.ServiceEventCategories.Add(new ServiceEventCategory
            {
                ServiceId = service.Id,
                EventCategoryId = categoryId
            });
        }

        await _serviceRepository.UpdateAsync(service);

        var updatedService = await _serviceRepository.GetByIdAsync(service.Id)
            ?? throw new InvalidOperationException("No se pudo recuperar el servicio actualizado.");

        return MapToDto(updatedService);
    }

    public async Task DeleteAsync(Guid serviceId, Guid currentUserId, bool isAdmin)
    {
        var service = await _serviceRepository.GetByIdAsync(serviceId)
            ?? throw new KeyNotFoundException("Servicio no encontrado.");

        if (!isAdmin && service.VendorId != currentUserId)
            throw new UnauthorizedAccessException("No tenés permiso para borrar este servicio.");

        _fileStorageService.DeleteMany(GetImageUrls(service.ImageUrlsJson));

        await using var transaction = await _context.Database.BeginTransactionAsync();

        var deleted = await _serviceRepository.DeleteAsync(serviceId);
        if (!deleted)
            throw new InvalidOperationException("No se pudo eliminar el servicio.");

        var remainingServices = await _serviceRepository.CountByVendorIdAsync(service.VendorId);
        if (remainingServices == 0)
        {
            var user = await _userRepository.GetByIdAsync(service.VendorId);
            if (user != null && !string.Equals(user.Rol, "administrador", StringComparison.OrdinalIgnoreCase))
            {
                user.Rol = "usuario";
                user.FechaActualizacion = DateTime.UtcNow;
                await _userRepository.UpdateAsync(user);
            }
        }

        await transaction.CommitAsync();
    }

    private static NormalizedServiceDto NormalizeAndValidate(CreateServiceDto dto)
    {
        if (dto.Direccion == null || dto.Direccion.DepartamentoId is null || dto.Direccion.BarrioId is null || string.IsNullOrWhiteSpace(dto.Direccion.Calle))
            throw new ArgumentException("La dirección es obligatoria y debe incluir departamento, barrio y calle.");

        if (dto.PrecioMinimo > dto.PrecioMaximo)
            throw new ArgumentException("El precio mínimo no puede ser mayor al precio máximo.");

        var tipoServicio = dto.TipoServicio.Trim();
        var isSalon = string.Equals(tipoServicio, "Salón", StringComparison.OrdinalIgnoreCase)
            || string.Equals(tipoServicio, "Salon", StringComparison.OrdinalIgnoreCase);

        if (isSalon && !dto.Capacidad.HasValue)
            throw new ArgumentException("La capacidad es obligatoria para los salones.");

        return new NormalizedServiceDto
        {
            Nombre = dto.Nombre.Trim(),
            Descripcion = dto.Descripcion.Trim(),
            Ubicacion = dto.Ubicacion.Trim(),
            TipoServicio = tipoServicio,
            PrecioMinimo = dto.PrecioMinimo,
            PrecioMaximo = dto.PrecioMaximo,
            Capacidad = dto.Capacidad,
            Direccion = dto.Direccion,
            CategoryIds = dto.CategoryIds ?? new List<Guid>()
        };
    }

    private static Direccion BuildDireccion(NormalizedServiceDto dto) => new()
    {
        DepartamentoId = dto.Direccion.DepartamentoId!.Value,
        BarrioId = dto.Direccion.BarrioId!.Value,
        Calle = dto.Direccion.Calle.Trim()
    };

    private sealed class NormalizedServiceDto
    {
        public string Nombre { get; init; } = string.Empty;
        public string Descripcion { get; init; } = string.Empty;
        public string Ubicacion { get; init; } = string.Empty;
        public string TipoServicio { get; init; } = string.Empty;
        public decimal PrecioMinimo { get; init; }
        public decimal PrecioMaximo { get; init; }
        public int? Capacidad { get; init; }
        public DireccionInputDto Direccion { get; init; } = new();
        public List<Guid> CategoryIds { get; init; } = new();
    }

    public async Task<IEnumerable<ServiceDto>> SearchAsync(string? searchTerm, string? location, decimal? minPrice, decimal? maxPrice)
    {
        var services = await _serviceRepository.GetActiveAsync();

        var filtered = services.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.ToLowerInvariant();
            filtered = filtered.Where(s =>
                s.Nombre.ToLowerInvariant().Contains(term) ||
                s.Descripcion.ToLowerInvariant().Contains(term)
            );
        }

        if (!string.IsNullOrWhiteSpace(location))
        {
            var loc = location.ToLowerInvariant();
            filtered = filtered.Where(s => s.Ubicacion.ToLowerInvariant().Contains(loc));
        }

        if (minPrice.HasValue)
        {
            filtered = filtered.Where(s => s.PrecioMaximo >= minPrice.Value);
        }

        if (maxPrice.HasValue)
        {
            filtered = filtered.Where(s => s.PrecioMinimo <= maxPrice.Value);
        }

        return filtered.Select(MapToDto);
    }

    public async Task<IEnumerable<ServiceDto>> FilterByPriceAndTypeAsync(decimal? minPrice, decimal? maxPrice, string? tipoServicio, List<Guid>? categoryIds)
    {
        var services = await _serviceRepository.GetActiveAsync();

        var filtered = services.AsEnumerable();

        if (minPrice.HasValue)
        {
            filtered = filtered.Where(s => s.PrecioMaximo >= minPrice.Value);
        }

        if (maxPrice.HasValue)
        {
            filtered = filtered.Where(s => s.PrecioMinimo <= maxPrice.Value);
        }

        if (!string.IsNullOrWhiteSpace(tipoServicio))
        {
            var tipo = tipoServicio.ToLowerInvariant();
            filtered = filtered.Where(s => s.TipoServicio.ToLowerInvariant() == tipo);
        }

        if (categoryIds != null && categoryIds.Count > 0)
        {
            filtered = filtered.Where(s =>
                s.ServiceEventCategories.Any(sc => categoryIds.Contains(sc.EventCategoryId))
            );
        }

        return filtered.Select(MapToDto);
    }

    private static ServiceDto MapToDto(Service service) => new()
    {
        Id = service.Id,
        VendorId = service.VendorId,
        Nombre = service.Nombre,
        Descripcion = service.Descripcion,
        Ubicacion = service.Ubicacion,
        TipoServicio = service.TipoServicio,
        PrecioMinimo = service.PrecioMinimo,
        PrecioMaximo = service.PrecioMaximo,
        Capacidad = service.Capacidad,
        Activo = service.Activo,
        FechaCreacion = service.FechaCreacion,
        FechaActualizacion = service.FechaActualizacion,
        Vendor = service.Vendor == null ? null : new VendorDto
        {
            Id = service.Vendor.Id,
            Nombre = service.Vendor.Nombre,
            Email = service.Vendor.Email,
            Telefono = service.Vendor.Telefono,
            ProfileImageUrl = service.Vendor.ProfileImageUrl
        },
        Direccion = BuildDireccionDto(service.DireccionCompleta),
        Categorias = service.ServiceEventCategories?.Select(sc => new EventCategoryDto
        {
            Id = sc.EventCategory!.Id,
            Nombre = sc.EventCategory.Nombre,
            FechaCreacion = sc.EventCategory.FechaCreacion
        }).ToList() ?? new(),
        Reservas = service.Reservas?.Select(reserva => new ReservaDto
        {
            Id = reserva.Id,
            ServiceId = reserva.ServiceId,
            UserId = reserva.UserId,
            Confirmada = reserva.Confirmada,
            FechaReservaCliente = reserva.FechaReservaCliente,
            Usuario = reserva.User == null ? null : new UserDto
            {
                Id = reserva.User.Id,
                Nombre = reserva.User.Nombre,
                Telefono = reserva.User.Telefono,
                Email = reserva.User.Email,
                Rol = reserva.User.Rol,
                Activo = reserva.User.Activo,
                FechaCreacion = reserva.User.FechaCreacion,
                FechaActualizacion = reserva.User.FechaActualizacion
            }
        }).ToList() ?? new()
        , Imagenes = GetImageUrls(service.ImageUrlsJson)
    };

    private async Task<string?> BuildImageUrlsJsonAsync(IEnumerable<IFormFile>? images, string filePrefix)
    {
        var savedUrls = await _fileStorageService.SaveManyAsync(images, "services", filePrefix);
        return savedUrls.Count == 0 ? null : JsonSerializer.Serialize(savedUrls);
    }

    private static List<string> GetImageUrls(string? imageUrlsJson)
    {
        if (string.IsNullOrWhiteSpace(imageUrlsJson))
            return new List<string>();

        try
        {
            return JsonSerializer.Deserialize<List<string>>(imageUrlsJson) ?? new List<string>();
        }
        catch
        {
            return new List<string>();
        }
    }

    private static DireccionDto? BuildDireccionDto(Direccion? direccion)
    {
        if (direccion == null)
            return null;

        var departamentoDto = BuildDepartamentoDto(direccion.Departamento);
        var barrioDto = BuildBarrioDto(direccion.Barrio);

        return new DireccionDto
        {
            Id = direccion.Id,
            Calle = direccion.Calle,
            Departamento = departamentoDto,
            Barrio = barrioDto
        };
    }

    private static DepartamentoDto? BuildDepartamentoDto(Departamento? departamento)
    {
        if (departamento == null)
            return null;

        return new DepartamentoDto
        {
            Id = departamento.Id,
            Nombre = departamento.Nombre
        };
    }

    private static BarrioDto? BuildBarrioDto(Barrio? barrio)
    {
        if (barrio == null)
            return null;

        return new BarrioDto
        {
            Id = barrio.Id,
            DepartamentoId = barrio.DepartamentoId,
            Nombre = barrio.Nombre,
            Departamento = BuildDepartamentoDto(barrio.Departamento)
        };
    }
}
