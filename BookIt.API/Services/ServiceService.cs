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
    private readonly ApplicationDbContext _context;

    public ServiceService(
        IServiceRepository serviceRepository,
        IUserRepository userRepository,
        ApplicationDbContext context)
    {
        _serviceRepository = serviceRepository;
        _userRepository = userRepository;
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
            ImageUrlsJson = BuildImageUrlsJson(dto.Images),
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

        if (dto.Images != null)
        {
            service.ImageUrlsJson = BuildImageUrlsJson(dto.Images);
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
        Reservas = BuildReservationDtos(service),
        Visitas = BuildVisitaDtos(service),
        Imagenes = GetImageUrls(service.ImageUrlsJson),
        ServiciosAsociados = service.ServiciosAsociados?
            .Where(ss => ss.Servicio != null)
            .Select(ss => MapToServicioAsociadoDto(ss.Servicio!))
            .ToList() ?? new()
    };

    private static List<ReservaDto> BuildReservationDtos(Service service)
    {
        return service.Reservas?.Select(reserva => new ReservaDto
        {
            Id = reserva.Id,
            ServiceId = reserva.ServiceId,
            UserId = reserva.UserId,
            Confirmada = reserva.Confirmada,
            FechaReservaCliente = reserva.FechaReservaCliente,
            MontoAcordado = reserva.MontoAcordado,
            HorasReservadas = reserva.HorasReservadas,
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
            },
            Pagos = (reserva.Pagos ?? []).Select(p => new PagoDto
            {
                Id = p.Id,
                ReservaId = p.ReservaId,
                TipoPago = p.TipoPago,
                Importe = p.Importe,
                FechaPago = p.FechaPago,
                FechaCreacion = p.FechaCreacion,
                FechaActualizacion = p.FechaActualizacion
            }).ToList()
        }).OrderBy(reserva => reserva.FechaReservaCliente).ToList() ?? new List<ReservaDto>();
    }

    private static List<VisitaDto> BuildVisitaDtos(Service service)
    {
        return service.Visitas?.Select(visita => new VisitaDto
        {
            Id = visita.Id,
            ServiceId = visita.ServiceId,
            ServiceNombre = service.Nombre,
            UserId = visita.UserId,
            UserNombre = visita.User?.Nombre,
            UserEmail = visita.User?.Email,
            FechaHoraSolicitada = visita.FechaHoraSolicitada,
            Estado = visita.Estado,
            Mensaje = visita.Mensaje,
            FechaCreacion = visita.FechaCreacion
        }).OrderBy(visita => visita.FechaHoraSolicitada).ToList() ?? new List<VisitaDto>();
    }

    private static string? BuildImageUrlsJson(IEnumerable<string>? images)
    {
        var normalizedUrls = NormalizeImageUrls(images);
        return normalizedUrls.Count == 0 ? null : JsonSerializer.Serialize(normalizedUrls);
    }

    private static List<string> NormalizeImageUrls(IEnumerable<string>? images)
    {
        var normalizedUrls = new List<string>();

        foreach (var imageUrl in images ?? Enumerable.Empty<string>())
        {
            if (string.IsNullOrWhiteSpace(imageUrl))
                continue;

            if (!Uri.TryCreate(imageUrl.Trim(), UriKind.Absolute, out var parsedUri))
                throw new ArgumentException("Cada imagen debe ser una URL absoluta válida de Cloudinary.");

            normalizedUrls.Add(parsedUri.ToString());
        }

        return normalizedUrls;
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

    private static ServicioAsociadoDto MapToServicioAsociadoDto(Models.Service service) => new()
    {
        Id = service.Id,
        Nombre = service.Nombre,
        TipoServicio = service.TipoServicio,
        PrecioMinimo = service.PrecioMinimo,
        PrecioMaximo = service.PrecioMaximo,
        Descripcion = service.Descripcion
    };

    public async Task<IEnumerable<ServicioAsociadoDto>> GetServiciosAsociadosAsync(Guid salonId)
    {
        var salon = await _serviceRepository.GetByIdAsync(salonId)
            ?? throw new KeyNotFoundException("Salón no encontrado.");

        return salon.ServiciosAsociados
            .Where(ss => ss.Servicio != null)
            .Select(ss => MapToServicioAsociadoDto(ss.Servicio!))
            .ToList();
    }

    public async Task<ServicioAsociadoDto> AsociarServicioAsync(Guid salonId, Guid serviceId, Guid currentUserId)
    {
        var salon = await _serviceRepository.GetByIdAsync(salonId)
            ?? throw new KeyNotFoundException("Salón no encontrado.");

        var isSalon = string.Equals(salon.TipoServicio, "Salón", StringComparison.OrdinalIgnoreCase)
            || string.Equals(salon.TipoServicio, "Salon", StringComparison.OrdinalIgnoreCase);
        if (!isSalon)
            throw new ArgumentException("El servicio indicado no es un salón.");

        if (salon.VendorId != currentUserId)
            throw new UnauthorizedAccessException("No tenés permiso para gestionar este salón.");

        if (salonId == serviceId)
            throw new ArgumentException("No podés asociar el salón consigo mismo.");

        var servicio = await _serviceRepository.GetByIdAsync(serviceId)
            ?? throw new KeyNotFoundException("Servicio no encontrado.");

        if (servicio.VendorId != currentUserId)
            throw new ArgumentException("El servicio debe pertenecer al mismo dueño que el salón.");

        if (salon.ServiciosAsociados.Any(ss => ss.ServiceId == serviceId))
            throw new InvalidOperationException("Este servicio ya está asociado al salón.");

        var association = new Models.SalonService { SalonId = salonId, ServiceId = serviceId };
        _context.SalonServices.Add(association);
        await _context.SaveChangesAsync();

        return MapToServicioAsociadoDto(servicio);
    }

    public async Task QuitarServicioAsociadoAsync(Guid salonId, Guid serviceId, Guid currentUserId)
    {
        var salon = await _serviceRepository.GetByIdAsync(salonId)
            ?? throw new KeyNotFoundException("Salón no encontrado.");

        if (salon.VendorId != currentUserId)
            throw new UnauthorizedAccessException("No tenés permiso para gestionar este salón.");

        var association = salon.ServiciosAsociados.FirstOrDefault(ss => ss.ServiceId == serviceId)
            ?? throw new KeyNotFoundException("La asociación no existe.");

        _context.SalonServices.Remove(association);
        await _context.SaveChangesAsync();
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
