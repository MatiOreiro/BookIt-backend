using BookIt.API.Data;
using BookIt.API.DTOs;
using BookIt.API.Models;
using BookIt.API.Repositories.Interfaces;
using BookIt.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BookIt.API.Services;

public class ReservaService : IReservaService
{
    private readonly IReservaRepository _reservaRepository;
    private readonly IVisitaRepository _visitaRepository;
    private readonly IUserRepository _userRepository;
    private readonly ApplicationDbContext _context;

    public ReservaService(
        IReservaRepository reservaRepository,
        IVisitaRepository visitaRepository,
        IUserRepository userRepository,
        ApplicationDbContext context)
    {
        _reservaRepository = reservaRepository;
        _visitaRepository = visitaRepository;
        _userRepository = userRepository;
        _context = context;
    }

    public async Task<ReservaDto> CreateAsync(Guid currentUserId, CreateReservaDto dto)
    {
        var fechaSlot = NormalizeToHalfHourSlot(dto.FechaReservaCliente);

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

        var reserva = new Reserva
        {
            ServiceId = service.Id,
            UserId = currentUserId,
            Confirmada = false,
            FechaReservaCliente = fechaSlot
        };

        var created = await _reservaRepository.CreateAsync(reserva);
        created.Service = service;
        created.User = user;

        return MapToDto(created);
    }

    public async Task<IEnumerable<ReservaDto>> GetByUserIdAsync(Guid userId)
    {
        var reservas = await _reservaRepository.GetByUserIdAsync(userId);
        return reservas.Select(MapToDto);
    }

    public async Task<IEnumerable<ReservaDto>> GetByServiceIdAsync(Guid currentUserId, bool isAdmin, Guid serviceId)
    {
        var service = await _visitaRepository.GetServiceByIdAsync(serviceId)
            ?? throw new KeyNotFoundException("Servicio no encontrado.");

        if (!isAdmin && service.VendorId != currentUserId)
            throw new UnauthorizedAccessException("No tenés permiso para ver las reservas de este servicio.");

        var reservas = await _reservaRepository.GetByServiceIdAsync(serviceId);
        return reservas.Select(MapToDto);
    }

    public async Task<ReservaDto> CreateFromVisitaAsync(Guid currentUserId, bool isAdmin, Guid visitaId)
    {
        var visita = await _context.Visitas
            .Include(v => v.Service)
            .Include(v => v.User)
            .FirstOrDefaultAsync(v => v.Id == visitaId)
            ?? throw new KeyNotFoundException("Visita no encontrada.");

        if (!isAdmin && visita.Service?.VendorId != currentUserId)
            throw new UnauthorizedAccessException("No tenés permiso para convertir esta visita en reserva.");

        if (!string.Equals(visita.Estado, "Pendiente", StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("La visita ya fue procesada.");

        var slot = NormalizeToHalfHourSlot(visita.FechaHoraSolicitada);
        var hasOtherReservation = await _context.Reservas.AnyAsync(r =>
            r.ServiceId == visita.ServiceId &&
            r.FechaReservaCliente == slot);

        var hasOtherVisit = await _context.Visitas.AnyAsync(v =>
            v.ServiceId == visita.ServiceId &&
            v.FechaHoraSolicitada == slot &&
            v.Id != visitaId &&
            (v.Estado == "Pendiente" || v.Estado == "Confirmada"));

        if (hasOtherReservation || hasOtherVisit)
            throw new ArgumentException("Ya existe una visita o reserva para ese horario.");

        var reserva = new Reserva
        {
            ServiceId = visita.ServiceId,
            UserId = visita.UserId,
            Confirmada = true,
            FechaReservaCliente = slot,
            Service = visita.Service,
            User = visita.User
        };

        visita.Estado = "Confirmada";
        visita.FechaActualizacion = DateTime.UtcNow;

        var created = await _reservaRepository.CreateAsync(reserva);
        await _context.SaveChangesAsync();

        return MapToDto(created);
    }

    public async Task<ReservaDto> ConfirmAsync(Guid currentUserId, bool isAdmin, Guid reservaId)
    {
        var reserva = await _context.Reservas
            .Include(r => r.Service)
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.Id == reservaId)
            ?? throw new KeyNotFoundException("Reserva no encontrada.");

        if (!isAdmin && reserva.Service?.VendorId != currentUserId)
            throw new UnauthorizedAccessException("No tenés permiso para confirmar esta reserva.");

        reserva.Confirmada = true;
        await _context.SaveChangesAsync();

        return MapToDto(reserva);
    }

    public async Task RejectAsync(Guid currentUserId, bool isAdmin, Guid reservaId)
    {
        var reserva = await _context.Reservas
            .Include(r => r.Service)
            .FirstOrDefaultAsync(r => r.Id == reservaId)
            ?? throw new KeyNotFoundException("Reserva no encontrada.");

        if (!isAdmin && reserva.Service?.VendorId != currentUserId)
            throw new UnauthorizedAccessException("No tenés permiso para rechazar esta reserva.");

        _context.Reservas.Remove(reserva);
        await _context.SaveChangesAsync();
    }

    private async Task EnsureSlotAvailableAsync(Guid serviceId, DateTime slot)
    {
        var hasReserva = await _context.Reservas.AnyAsync(r => r.ServiceId == serviceId && r.FechaReservaCliente == slot);
        var hasVisita = await _context.Visitas.AnyAsync(v =>
            v.ServiceId == serviceId &&
            v.FechaHoraSolicitada == slot &&
            (v.Estado == "Pendiente" || v.Estado == "Confirmada"));

        if (hasReserva || hasVisita)
            throw new ArgumentException("Ya existe una visita o reserva para ese horario.");
    }

    private static DateTime NormalizeToHalfHourSlot(DateTime value)
    {
        var utcValue = value.Kind == DateTimeKind.Utc ? value : value.ToUniversalTime();
        var normalizedMinute = utcValue.Minute < 30 ? 0 : 30;
        return new DateTime(utcValue.Year, utcValue.Month, utcValue.Day, utcValue.Hour, normalizedMinute, 0, DateTimeKind.Utc);
    }

    private static ReservaDto MapToDto(Reserva reserva) => new()
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
    };
}
