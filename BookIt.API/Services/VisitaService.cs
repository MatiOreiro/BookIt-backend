using BookIt.API.Data;
using BookIt.API.DTOs;
using BookIt.API.Models;
using BookIt.API.Repositories.Interfaces;
using BookIt.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BookIt.API.Services;

public class VisitaService : IVisitaService
{
    private readonly IVisitaRepository _visitaRepository;
    private readonly IUserRepository _userRepository;
    private readonly ApplicationDbContext _context;

    public VisitaService(IVisitaRepository visitaRepository, IUserRepository userRepository, ApplicationDbContext context)
    {
        _visitaRepository = visitaRepository;
        _userRepository = userRepository;
        _context = context;
    }

    public async Task<VisitaDto> CreateAsync(Guid currentUserId, CreateVisitaDto dto)
    {
        var fechaSlot = NormalizeToHalfHourSlot(dto.FechaHoraSolicitada);

        if (fechaSlot <= DateTime.UtcNow)
            throw new ArgumentException("La fecha y hora solicitadas deben ser futuras.");

        if (!string.IsNullOrWhiteSpace(dto.Mensaje) && dto.Mensaje.Length > 500)
            throw new ArgumentException("El mensaje no puede superar los 500 caracteres.");

        var user = await _userRepository.GetByIdAsync(currentUserId)
            ?? throw new KeyNotFoundException("El usuario no existe.");

        var service = await _visitaRepository.GetServiceByIdAsync(dto.ServiceId)
            ?? throw new KeyNotFoundException("Servicio no encontrado.");

        if (!service.Activo)
            throw new ArgumentException("El servicio no está activo.");

        await EnsureSlotAvailableAsync(service.Id, fechaSlot);

        var visita = new Visita
        {
            ServiceId = service.Id,
            UserId = currentUserId,
            FechaHoraSolicitada = fechaSlot,
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

    public async Task<VisitaDto> UpdateEstadoAsync(Guid currentUserId, bool isAdmin, Guid visitaId, string estado)
    {
        var visita = await _context.Visitas
            .Include(v => v.Service)
            .Include(v => v.User)
            .FirstOrDefaultAsync(v => v.Id == visitaId)
            ?? throw new KeyNotFoundException("Visita no encontrada.");

        if (!isAdmin && visita.Service?.VendorId != currentUserId)
            throw new UnauthorizedAccessException("No tenés permiso para actualizar esta visita.");

        if (string.Equals(estado, "Confirmada", StringComparison.OrdinalIgnoreCase)
            && visita.FechaHoraSolicitada > DateTime.UtcNow)
        {
            throw new ArgumentException("La visita solo se puede marcar como cumplida cuando ya pasó su fecha y hora.");
        }

        visita.Estado = estado;
        visita.FechaActualizacion = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return MapToDto(visita);
    }

    public async Task DeleteAsync(Guid currentUserId, bool isAdmin, Guid visitaId)
    {
        var visita = await _context.Visitas
            .Include(v => v.Service)
            .FirstOrDefaultAsync(v => v.Id == visitaId)
            ?? throw new KeyNotFoundException("Visita no encontrada.");

        if (!isAdmin && visita.Service?.VendorId != currentUserId)
            throw new UnauthorizedAccessException("No tenés permiso para eliminar esta visita.");

        _context.Visitas.Remove(visita);
        await _context.SaveChangesAsync();
    }

    private async Task EnsureSlotAvailableAsync(Guid serviceId, DateTime slot)
    {
        var hasVisit = await _context.Visitas.AnyAsync(v =>
            v.ServiceId == serviceId &&
            v.FechaHoraSolicitada == slot &&
            (v.Estado == "Pendiente" || v.Estado == "Confirmada"));

        if (hasVisit)
            throw new ArgumentException("Ya existe una visita o reserva para ese horario.");

        var hasUnconfirmedReserva = await _context.Reservas.AnyAsync(r =>
            r.ServiceId == serviceId &&
            r.FechaReservaCliente == slot &&
            !r.Confirmada);

        if (hasUnconfirmedReserva)
            throw new ArgumentException("Ya existe una reserva pendiente para ese horario.");

        var confirmedReservas = await _context.Reservas
            .Where(r => r.ServiceId == serviceId && r.Confirmada)
            .Select(r => new { r.FechaReservaCliente, r.HorasReservadas })
            .ToListAsync();

        var overlaps = confirmedReservas.Any(r =>
        {
            var end = r.HorasReservadas.HasValue
                ? r.FechaReservaCliente.AddHours((double)r.HorasReservadas.Value)
                : r.FechaReservaCliente.AddMinutes(30);
            return slot >= r.FechaReservaCliente && slot < end;
        });

        if (overlaps)
            throw new ArgumentException("El horario solicitado se solapa con una reserva confirmada.");
    }

    private static DateTime NormalizeToHalfHourSlot(DateTime value)
    {
        var utcValue = value.Kind == DateTimeKind.Utc ? value : value.ToUniversalTime();
        var normalizedMinute = utcValue.Minute < 30 ? 0 : 30;
        return new DateTime(utcValue.Year, utcValue.Month, utcValue.Day, utcValue.Hour, normalizedMinute, 0, DateTimeKind.Utc);
    }

    private static VisitaDto MapToDto(Visita visita) => new()
    {
        Id = visita.Id,
        ServiceId = visita.ServiceId,
        ServiceNombre = visita.Service?.Nombre,
        UserId = visita.UserId,
        UserNombre = visita.User?.Nombre,
        UserEmail = visita.User?.Email,
        FechaHoraSolicitada = visita.FechaHoraSolicitada,
        Estado = visita.Estado,
        Mensaje = visita.Mensaje,
        FechaCreacion = visita.FechaCreacion,
        VendorNombre = visita.Service?.Vendor?.Nombre,
        VendorEmail = visita.Service?.Vendor?.Email,
        VendorTelefono = visita.Service?.Vendor?.Telefono
    };
}
