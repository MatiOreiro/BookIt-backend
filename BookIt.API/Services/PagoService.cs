// BookIt-backend/BookIt.API/Services/PagoService.cs
using BookIt.API.Data;
using BookIt.API.DTOs;
using BookIt.API.Models;
using BookIt.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BookIt.API.Services;

public class PagoService : IPagoService
{
    private static readonly string[] TiposPagoValidos = ["Seña", "Parcial", "Total"];
    private readonly ApplicationDbContext _context;

    public PagoService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PagoDto> CreateAsync(Guid currentUserId, bool isAdmin, CreatePagoDto dto)
    {
        var reserva = await _context.Reservas
            .Include(r => r.Service)
            .FirstOrDefaultAsync(r => r.Id == dto.ReservaId)
            ?? throw new KeyNotFoundException("Reserva no encontrada.");

        if (!isAdmin && reserva.Service?.VendorId != currentUserId)
            throw new UnauthorizedAccessException("No tenés permiso para registrar pagos en esta reserva.");

        if (!reserva.Confirmada)
            throw new ArgumentException("Solo se pueden registrar pagos en reservas confirmadas.");

        if (!TiposPagoValidos.Contains(dto.TipoPago))
            throw new ArgumentException("El tipo de pago debe ser Seña, Parcial o Total.");

        var pago = new Pago
        {
            ReservaId = dto.ReservaId,
            TipoPago = dto.TipoPago,
            Importe = dto.Importe,
            FechaPago = dto.FechaPago.Kind == DateTimeKind.Utc
                ? dto.FechaPago
                : DateTime.SpecifyKind(dto.FechaPago, DateTimeKind.Utc)
        };

        _context.Pagos.Add(pago);
        await _context.SaveChangesAsync();

        return MapToDto(pago);
    }

    public async Task<PagoDto> UpdateAsync(Guid currentUserId, bool isAdmin, Guid pagoId, UpdatePagoDto dto)
    {
        var pago = await _context.Pagos
            .Include(p => p.Reserva)
                .ThenInclude(r => r!.Service)
            .FirstOrDefaultAsync(p => p.Id == pagoId)
            ?? throw new KeyNotFoundException("Pago no encontrado.");

        if (!isAdmin && pago.Reserva?.Service?.VendorId != currentUserId)
            throw new UnauthorizedAccessException("No tenés permiso para editar este pago.");

        if (pago.Reserva?.Confirmada != true)
            throw new ArgumentException("Solo se pueden editar pagos en reservas confirmadas.");

        if (!TiposPagoValidos.Contains(dto.TipoPago))
            throw new ArgumentException("El tipo de pago debe ser Seña, Parcial o Total.");

        pago.TipoPago = dto.TipoPago;
        pago.Importe = dto.Importe;
        pago.FechaPago = dto.FechaPago.Kind == DateTimeKind.Utc
            ? dto.FechaPago
            : DateTime.SpecifyKind(dto.FechaPago, DateTimeKind.Utc);
        pago.FechaActualizacion = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return MapToDto(pago);
    }

    public async Task<IEnumerable<PagoDto>> GetByReservaIdAsync(Guid currentUserId, bool isAdmin, Guid reservaId)
    {
        var reserva = await _context.Reservas
            .Include(r => r.Service)
            .FirstOrDefaultAsync(r => r.Id == reservaId)
            ?? throw new KeyNotFoundException("Reserva no encontrada.");

        if (!isAdmin && reserva.Service?.VendorId != currentUserId)
            throw new UnauthorizedAccessException("No tenés permiso para ver los pagos de esta reserva.");

        var pagos = await _context.Pagos
            .Where(p => p.ReservaId == reservaId)
            .OrderBy(p => p.FechaPago)
            .ToListAsync();

        return pagos.Select(MapToDto);
    }

    private static PagoDto MapToDto(Pago pago) => new()
    {
        Id = pago.Id,
        ReservaId = pago.ReservaId,
        TipoPago = pago.TipoPago,
        Importe = pago.Importe,
        FechaPago = pago.FechaPago,
        FechaCreacion = pago.FechaCreacion,
        FechaActualizacion = pago.FechaActualizacion
    };
}
