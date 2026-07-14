// BookIt-backend/BookIt.API/Services/ReservaService.cs
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

        await EnsureSlotNotInConfirmedRangeAsync(service.Id, fechaSlot);

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

    public async Task<ReservaDto> CreateFromVisitaAsync(Guid currentUserId, bool isAdmin, Guid visitaId, ConfirmarVisitaDto dto)
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

        var rawFecha = dto.FechaReservaCliente ?? visita.FechaHoraSolicitada;
        var slot = NormalizeToHalfHourSlot(rawFecha);

        await EnsureSlotNotInConfirmedRangeAsync(visita.ServiceId, slot, visita.Id);

        var hasOtherVisit = await _context.Visitas.AnyAsync(v =>
            v.ServiceId == visita.ServiceId &&
            v.FechaHoraSolicitada == slot &&
            v.Id != visitaId &&
            (v.Estado == "Pendiente" || v.Estado == "Confirmada"));

        if (hasOtherVisit)
            throw new ArgumentException("Ya existe una visita para ese horario.");

        var reserva = new Reserva
        {
            ServiceId = visita.ServiceId,
            UserId = visita.UserId,
            Confirmada = true,
            FechaReservaCliente = slot,
            HorasReservadas = dto.HorasReservadas,
            MontoAcordado = dto.MontoAcordado,
            Service = visita.Service,
            User = visita.User
        };

        visita.Estado = "Confirmada";
        visita.FechaActualizacion = DateTime.UtcNow;

        var created = await _reservaRepository.CreateAsync(reserva);
        await _context.SaveChangesAsync();

        return MapToDto(created);
    }

    public async Task<ReservaDto> ConfirmAsync(Guid currentUserId, bool isAdmin, Guid reservaId, ConfirmarReservaDto dto)
    {
        var reserva = await _context.Reservas
            .Include(r => r.Service)
            .Include(r => r.User)
            .Include(r => r.Pagos)
            .FirstOrDefaultAsync(r => r.Id == reservaId)
            ?? throw new KeyNotFoundException("Reserva no encontrada.");

        if (!isAdmin && reserva.Service?.VendorId != currentUserId)
            throw new UnauthorizedAccessException("No tenés permiso para confirmar esta reserva.");

        if (reserva.Confirmada)
            throw new InvalidOperationException("La reserva ya está confirmada. Usá el endpoint de actualización financiera.");

        await EnsureRangeAvailableAsync(reserva.ServiceId, reservaId, reserva.FechaReservaCliente, dto.HorasReservadas);

        reserva.Confirmada = true;
        reserva.HorasReservadas = dto.HorasReservadas;
        reserva.MontoAcordado = dto.MontoAcordado;
        await _context.SaveChangesAsync();

        return MapToDto(reserva);
    }

    public async Task<ReservaDto> UpdateFinancieroAsync(Guid currentUserId, bool isAdmin, Guid reservaId, ConfirmarReservaDto dto)
    {
        var reserva = await _context.Reservas
            .Include(r => r.Service)
            .Include(r => r.User)
            .Include(r => r.Pagos)
            .FirstOrDefaultAsync(r => r.Id == reservaId)
            ?? throw new KeyNotFoundException("Reserva no encontrada.");

        if (!isAdmin && reserva.Service?.VendorId != currentUserId)
            throw new UnauthorizedAccessException("No tenés permiso para actualizar esta reserva.");

        if (!reserva.Confirmada)
            throw new ArgumentException("Solo se pueden actualizar datos financieros de reservas confirmadas.");

        await EnsureRangeAvailableAsync(reserva.ServiceId, reservaId, reserva.FechaReservaCliente, dto.HorasReservadas);

        reserva.HorasReservadas = dto.HorasReservadas;
        reserva.MontoAcordado = dto.MontoAcordado;
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

    // Checks that the given slot does not fall inside any confirmed reservation's time range.
    // Reservations without HorasReservadas block only their exact 30-min start slot.
    private async Task EnsureSlotNotInConfirmedRangeAsync(Guid serviceId, DateTime slot, Guid? excludeVisitaId = null)
    {
        var hasVisita = await _context.Visitas.AnyAsync(v =>
            v.ServiceId == serviceId &&
            v.FechaHoraSolicitada == slot &&
            (v.Estado == "Pendiente" || v.Estado == "Confirmada") &&
            (excludeVisitaId == null || v.Id != excludeVisitaId));

        if (hasVisita)
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

    // Checks that [start, start+hours) does not overlap any other confirmed reservation's range.
    private async Task EnsureRangeAvailableAsync(Guid serviceId, Guid excludeReservaId, DateTime start, decimal hours)
    {
        var end = start.AddHours((double)hours);

        var confirmedReservas = await _context.Reservas
            .Where(r => r.ServiceId == serviceId && r.Confirmada && r.Id != excludeReservaId)
            .Select(r => new { r.FechaReservaCliente, r.HorasReservadas })
            .ToListAsync();

        var overlaps = confirmedReservas.Any(r =>
        {
            var rEnd = r.HorasReservadas.HasValue
                ? r.FechaReservaCliente.AddHours((double)r.HorasReservadas.Value)
                : r.FechaReservaCliente.AddMinutes(30);
            return start < rEnd && r.FechaReservaCliente < end;
        });

        if (overlaps)
            throw new ArgumentException("El rango de horas solicitado se solapa con otra reserva confirmada.");
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
        MontoAcordado = reserva.MontoAcordado,
        HorasReservadas = reserva.HorasReservadas,
        Realizada = reserva.EsRealizada(),
        ResenaId = reserva.Resena?.Id,
        ServiceNombre = reserva.Service?.Nombre,
        PrecioMinimo = reserva.Service?.PrecioMinimo ?? 0,
        PrecioMaximo = reserva.Service?.PrecioMaximo ?? 0,
        VendorNombre = reserva.Service?.Vendor?.Nombre,
        VendorEmail = reserva.Service?.Vendor?.Email,
        VendorTelefono = reserva.Service?.Vendor?.Telefono,
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
    };
}
