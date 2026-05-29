using BookIt.API.DTOs;
using BookIt.API.Models;
using BookIt.API.Repositories.Interfaces;
using BookIt.API.Services.Interfaces;

namespace BookIt.API.Services;

public class VisitaService : IVisitaService
{
    private readonly IVisitaRepository _visitaRepository;
    private readonly IUserRepository _userRepository;

    public VisitaService(IVisitaRepository visitaRepository, IUserRepository userRepository)
    {
        _visitaRepository = visitaRepository;
        _userRepository = userRepository;
    }

    public async Task<VisitaDto> CreateAsync(Guid currentUserId, CreateVisitaDto dto)
    {
        var fechaUtc = dto.FechaHoraSolicitada.Kind == DateTimeKind.Utc
            ? dto.FechaHoraSolicitada
            : dto.FechaHoraSolicitada.ToUniversalTime();

        if (fechaUtc <= DateTime.UtcNow)
            throw new ArgumentException("La fecha y hora solicitadas deben ser futuras.");

        if (!string.IsNullOrWhiteSpace(dto.Mensaje) && dto.Mensaje.Length > 500)
            throw new ArgumentException("El mensaje no puede superar los 500 caracteres.");

        var user = await _userRepository.GetByIdAsync(currentUserId)
            ?? throw new KeyNotFoundException("El usuario no existe.");

        var service = await _visitaRepository.GetServiceByIdAsync(dto.ServiceId)
            ?? throw new KeyNotFoundException("Servicio no encontrado.");

        if (!service.Activo)
            throw new ArgumentException("El servicio no está activo.");

        var hasDuplicate = await _visitaRepository.ExistsPendingOrConfirmedAsync(dto.ServiceId, fechaUtc);
        if (hasDuplicate)
            throw new ArgumentException("Ya existe una visita pendiente o confirmada para ese horario.");

        var visita = new Visita
        {
            ServiceId = service.Id,
            UserId = currentUserId,
            FechaHoraSolicitada = fechaUtc,
            Estado = "Pendiente",
            Mensaje = string.IsNullOrWhiteSpace(dto.Mensaje) ? null : dto.Mensaje.Trim(),
            FechaCreacion = DateTime.UtcNow,
            FechaActualizacion = DateTime.UtcNow
        };

        var created = await _visitaRepository.CreateAsync(visita);
        created.Service = service;
        created.User = user;

        return MapToDto(created);
    }

    public async Task<IEnumerable<VisitaDto>> GetByUserIdAsync(Guid userId)
    {
        var visitas = await _visitaRepository.GetByUserIdAsync(userId);
        return visitas.Select(MapToDto);
    }

    public async Task<IEnumerable<VisitaDto>> GetByServiceIdAsync(Guid currentUserId, bool isAdmin, Guid serviceId)
    {
        var service = await _visitaRepository.GetServiceByIdAsync(serviceId)
            ?? throw new KeyNotFoundException("Servicio no encontrado.");

        if (!isAdmin && service.VendorId != currentUserId)
            throw new UnauthorizedAccessException("No tenés permiso para ver las visitas de este servicio.");

        var visitas = await _visitaRepository.GetByServiceIdAsync(serviceId);
        return visitas.Select(MapToDto);
    }

    private static VisitaDto MapToDto(Visita visita) => new()
    {
        Id = visita.Id,
        ServiceId = visita.ServiceId,
        ServiceNombre = visita.Service?.Nombre,
        UserId = visita.UserId,
        UserNombre = visita.User?.Nombre,
        FechaHoraSolicitada = visita.FechaHoraSolicitada,
        Estado = visita.Estado,
        Mensaje = visita.Mensaje,
        FechaCreacion = visita.FechaCreacion
    };
}
