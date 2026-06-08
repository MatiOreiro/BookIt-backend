using BookIt.API.Data;
using BookIt.API.Models;
using BookIt.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BookIt.API.Repositories;

public class ReservaRepository : IReservaRepository
{
    private readonly ApplicationDbContext _context;

    public ReservaRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Reserva> CreateAsync(Reserva reserva)
    {
        _context.Reservas.Add(reserva);
        await _context.SaveChangesAsync();
        return reserva;
    }

    public async Task<Reserva?> GetByIdAsync(Guid id)
    {
        return await _context.Reservas
            .Include(r => r.Service)
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<IEnumerable<Reserva>> GetByUserIdAsync(Guid userId)
    {
        return await _context.Reservas
            .Include(r => r.Service)
            .Include(r => r.User)
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.FechaReservaCliente)
            .ToListAsync();
    }

    public async Task<IEnumerable<Reserva>> GetByServiceIdAsync(Guid serviceId)
    {
        return await _context.Reservas
            .Include(r => r.Service)
            .Include(r => r.User)
            .Where(r => r.ServiceId == serviceId)
            .OrderByDescending(r => r.FechaReservaCliente)
            .ToListAsync();
    }

    public async Task<bool> ExistsBusySlotAsync(Guid serviceId, DateTime fechaReservaCliente)
    {
        return await _context.Reservas.AnyAsync(r =>
            r.ServiceId == serviceId &&
            r.FechaReservaCliente == fechaReservaCliente);
    }
}
