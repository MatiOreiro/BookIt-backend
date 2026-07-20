using BookIt.API.Data;
using BookIt.API.DTOs;
using BookIt.API.Models;
using BookIt.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BookIt.API.Services;

public class PropuestaService : IPropuestaService
{
    private readonly ApplicationDbContext _context;

    public PropuestaService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PropuestaDto> CreateAsync(Guid currentUserId, CreatePropuestaDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Nombre))
            throw new ArgumentException("El nombre de la propuesta es obligatorio.");

        if (dto.ServiceIds == null || dto.ServiceIds.Count == 0)
            throw new ArgumentException("La propuesta debe incluir al menos un servicio.");

        if (dto.ServiceIds.Distinct().Count() != dto.ServiceIds.Count)
            throw new ArgumentException("No se puede repetir el mismo servicio en la propuesta.");

        if (dto.ServiceIds.Contains(dto.SalonId))
            throw new ArgumentException("El salón no puede figurar también como servicio.");

        var salon = await _context.Services
            .Include(s => s.Vendor)
            .FirstOrDefaultAsync(s => s.Id == dto.SalonId)
            ?? throw new KeyNotFoundException("Salón no encontrado.");

        if (!IsSalonTipo(salon.TipoServicio))
            throw new ArgumentException("El servicio indicado como salón no es un salón.");

        if (!salon.Activo)
            throw new ArgumentException("El salón indicado no está activo.");

        var servicios = await _context.Services
            .Include(s => s.Vendor)
            .Where(s => dto.ServiceIds.Contains(s.Id))
            .ToListAsync();

        if (servicios.Count != dto.ServiceIds.Count)
            throw new KeyNotFoundException("Alguno de los servicios indicados no existe.");

        if (servicios.Any(s => IsSalonTipo(s.TipoServicio)))
            throw new ArgumentException("Los servicios de la propuesta no pueden ser salones.");

        if (servicios.Any(s => !s.Activo))
            throw new ArgumentException("Todos los servicios deben estar activos.");

        var propuesta = new Propuesta
        {
            UserId = currentUserId,
            Nombre = dto.Nombre.Trim(),
            SalonId = salon.Id
        };

        propuesta.Servicios = dto.ServiceIds
            .Select(serviceId => new PropuestaServicio { PropuestaId = propuesta.Id, ServiceId = serviceId })
            .ToList();

        _context.Propuestas.Add(propuesta);
        await _context.SaveChangesAsync();

        return MapToDto(propuesta, salon, servicios);
    }

    public async Task<IEnumerable<PropuestaDto>> GetByUserIdAsync(Guid userId)
    {
        var propuestas = await _context.Propuestas
            .Include(p => p.Salon).ThenInclude(s => s!.Vendor)
            .Include(p => p.Servicios).ThenInclude(ps => ps.Service).ThenInclude(s => s!.Vendor)
            .Where(p => p.UserId == userId)
            .OrderByDescending(p => p.FechaCreacion)
            .ToListAsync();

        return propuestas.Select(p => MapToDto(p, p.Salon!, p.Servicios.Select(ps => ps.Service!).ToList()));
    }

    public async Task DeleteAsync(Guid currentUserId, Guid propuestaId)
    {
        var propuesta = await _context.Propuestas.FirstOrDefaultAsync(p => p.Id == propuestaId)
            ?? throw new KeyNotFoundException("Propuesta no encontrada.");

        if (propuesta.UserId != currentUserId)
            throw new UnauthorizedAccessException("No tenés permiso para eliminar esta propuesta.");

        _context.Propuestas.Remove(propuesta);
        await _context.SaveChangesAsync();
    }

    private static PropuestaDto MapToDto(Propuesta propuesta, Service salon, List<Service> servicios) => new()
    {
        Id = propuesta.Id,
        Nombre = propuesta.Nombre,
        FechaCreacion = propuesta.FechaCreacion,
        Salon = MapToItemDto(salon),
        Servicios = servicios.Select(MapToItemDto).ToList(),
        TotalEstimado = salon.PrecioMinimo + servicios.Sum(s => s.PrecioMinimo)
    };

    private static PropuestaItemDto MapToItemDto(Service service) => new()
    {
        Id = service.Id,
        Nombre = service.Nombre,
        TipoServicio = service.TipoServicio,
        PrecioMinimo = service.PrecioMinimo,
        VendorNombre = service.Vendor?.Nombre,
        VendorEmail = service.Vendor?.Email,
        VendorTelefono = service.Vendor?.Telefono
    };

    private static bool IsSalonTipo(string tipoServicio) =>
        string.Equals(tipoServicio, "Salón", StringComparison.OrdinalIgnoreCase)
        || string.Equals(tipoServicio, "Salon", StringComparison.OrdinalIgnoreCase);
}
