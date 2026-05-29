using BookIt.API.Data;
using BookIt.API.Models;
using BookIt.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BookIt.API.Repositories;

public class ServiceRepository : IServiceRepository
{
    private readonly ApplicationDbContext _context;

    public ServiceRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Service?> GetByIdAsync(Guid id)
    {
        return await _context.Services
            .Include(s => s.Vendor)
            .Include(s => s.DireccionCompleta).ThenInclude(d => d!.Departamento)
            .Include(s => s.DireccionCompleta).ThenInclude(d => d!.Barrio)
            .Include(s => s.ServiceEventCategories)
                .ThenInclude(sc => sc.EventCategory)
            .Include(s => s.Reservas)
                .ThenInclude(r => r.User)
            .Include(s => s.Visitas)
                .ThenInclude(v => v.User)
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<IEnumerable<Service>> GetByVendorIdAsync(Guid vendorId)
    {
        return await _context.Services
            .Include(s => s.Vendor)
            .Include(s => s.DireccionCompleta).ThenInclude(d => d!.Departamento)
            .Include(s => s.DireccionCompleta).ThenInclude(d => d!.Barrio)
            .Include(s => s.ServiceEventCategories)
                .ThenInclude(sc => sc.EventCategory)
            .Include(s => s.Reservas)
                .ThenInclude(r => r.User)
            .Include(s => s.Visitas)
                .ThenInclude(v => v.User)
            .Where(s => s.VendorId == vendorId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Service>> GetAllAsync()
    {
        return await _context.Services
            .Include(s => s.Vendor)
            .Include(s => s.DireccionCompleta).ThenInclude(d => d!.Departamento)
            .Include(s => s.DireccionCompleta).ThenInclude(d => d!.Barrio)
            .Include(s => s.ServiceEventCategories)
                .ThenInclude(sc => sc.EventCategory)
            .Include(s => s.Reservas)
                .ThenInclude(r => r.User)
            .Include(s => s.Visitas)
                .ThenInclude(v => v.User)
            .ToListAsync();
    }

    public async Task<IEnumerable<Service>> GetActiveAsync()
    {
        return await _context.Services
            .Include(s => s.Vendor)
            .Include(s => s.DireccionCompleta).ThenInclude(d => d!.Departamento)
            .Include(s => s.DireccionCompleta).ThenInclude(d => d!.Barrio)
            .Include(s => s.ServiceEventCategories)
                .ThenInclude(sc => sc.EventCategory)
            .Include(s => s.Reservas)
                .ThenInclude(r => r.User)
            .Include(s => s.Visitas)
                .ThenInclude(v => v.User)
            .Where(s => s.Activo)
            .ToListAsync();
    }

    public async Task<Service> CreateAsync(Service service)
    {
        _context.Services.Add(service);
        await _context.SaveChangesAsync();
        return service;
    }

    public async Task<Service> UpdateAsync(Service service)
    {
        _context.Services.Update(service);
        await _context.SaveChangesAsync();
        return service;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var service = await _context.Services.FindAsync(id);
        if (service == null) return false;

        _context.Services.Remove(service);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ExistsByIdAsync(Guid id)
    {
        return await _context.Services.AnyAsync(s => s.Id == id);
    }

    public async Task<int> CountByVendorIdAsync(Guid vendorId)
    {
        return await _context.Services.CountAsync(s => s.VendorId == vendorId);
    }
}
