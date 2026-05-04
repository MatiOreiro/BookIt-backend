using BookIt.API.DTOs;
using BookIt.API.Models;
using BookIt.API.Repositories.Interfaces;
using BookIt.API.Services.Interfaces;

namespace BookIt.API.Services;

public class ServiceService : IServiceService
{
    private readonly IServiceRepository _serviceRepository;

    public ServiceService(IServiceRepository serviceRepository)
    {
        _serviceRepository = serviceRepository;
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
        if (services == null)
            return Enumerable.Empty<ServiceDto>();
        
        return new[] { services }.Select(MapToDto);
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

    private static ServiceDto MapToDto(Service service) => new()
    {
        Id = service.Id,
        VendorId = service.VendorId,
        Nombre = service.Nombre,
        Descripcion = service.Descripcion,
        Ubicacion = service.Ubicacion,
        PrecioMinimo = service.PrecioMinimo,
        PrecioMaximo = service.PrecioMaximo,
        Activo = service.Activo,
        FechaCreacion = service.FechaCreacion,
        FechaActualizacion = service.FechaActualizacion,
        Vendor = service.Vendor == null ? null : new VendorDto
        {
            Id = service.Vendor.Id,
            Nombre = service.Vendor.Nombre,
            Email = service.Vendor.Email,
            Telefono = service.Vendor.Telefono
        }
    };
}
