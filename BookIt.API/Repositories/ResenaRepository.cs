using BookIt.API.Data;
using BookIt.API.Models;
using BookIt.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BookIt.API.Repositories;

public class ResenaRepository : IResenaRepository
{
    private readonly ApplicationDbContext _context;

    public ResenaRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Resena> CreateAsync(Resena resena)
    {
        _context.Resenas.Add(resena);
        await _context.SaveChangesAsync();
        return resena;
    }

    public async Task<IEnumerable<Resena>> GetByServiceIdAsync(Guid serviceId)
    {
        return await _context.Resenas
            .Include(r => r.User)
            .Where(r => r.ServiceId == serviceId)
            .OrderByDescending(r => r.FechaCreacion)
            .ToListAsync();
    }
}
