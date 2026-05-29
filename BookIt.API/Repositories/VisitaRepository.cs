using BookIt.API.Data;
using BookIt.API.Models;
using BookIt.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BookIt.API.Repositories;

public class VisitaRepository : IVisitaRepository
{
    private readonly ApplicationDbContext _context;

    public VisitaRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Visita> CreateAsync(Visita visita)
    {
        _context.Visitas.Add(visita);
        await _context.SaveChangesAsync();
        return visita;
    }

    public async Task<Service?> GetServiceByIdAsync(Guid serviceId)
    {
        return await _context.Services
            .Include(s => s.Vendor)
            .FirstOrDefaultAsync(s => s.Id == serviceId);
    }

    public async Task<bool> ExistsPendingOrConfirmedAsync(Guid serviceId, DateTime fechaHoraSolicitada)
    {
        return await _context.Visitas.AnyAsync(v =>
            v.ServiceId == serviceId
            && v.FechaHoraSolicitada == fechaHoraSolicitada
            && (v.Estado == "Pendiente" || v.Estado == "Confirmada"));
    }

    public async Task<IEnumerable<Visita>> GetByUserIdAsync(Guid userId)
    {
        return await _context.Visitas
            .Include(v => v.Service)
            .Include(v => v.User)
            .Where(v => v.UserId == userId)
            .OrderByDescending(v => v.FechaCreacion)
            .ToListAsync();
    }

    public async Task<IEnumerable<Visita>> GetByServiceIdAsync(Guid serviceId)
    {
        return await _context.Visitas
            .Include(v => v.Service)
            .Include(v => v.User)
            .Where(v => v.ServiceId == serviceId)
            .OrderByDescending(v => v.FechaCreacion)
            .ToListAsync();
    }
}
