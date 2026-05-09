using BookIt.API.Models;
using Microsoft.EntityFrameworkCore;

namespace BookIt.API.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Service> Services { get; set; }
    public DbSet<Tag> Tags { get; set; }
    public DbSet<ServiceTag> ServiceTags { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(u => u.Id);
            entity.HasIndex(u => u.Email).IsUnique();
            entity.Property(u => u.Nombre).IsRequired().HasMaxLength(100);
            entity.Property(u => u.Telefono).IsRequired().HasMaxLength(20);
            entity.Property(u => u.Email).IsRequired().HasMaxLength(200);
            entity.Property(u => u.PasswordHash).IsRequired();
            entity.Property(u => u.Rol).IsRequired().HasMaxLength(50);
        });

        modelBuilder.Entity<Service>(entity =>
        {
            entity.HasKey(s => s.Id);
            entity.HasIndex(s => s.VendorId);
            entity.Property(s => s.Nombre).IsRequired().HasMaxLength(150);
            entity.Property(s => s.Descripcion).IsRequired().HasMaxLength(1000);
            entity.Property(s => s.Ubicacion).IsRequired().HasMaxLength(255);
            entity.Property(s => s.TipoServicio).IsRequired().HasMaxLength(50);
            entity.Property(s => s.PrecioMinimo).HasPrecision(10, 2);
            entity.Property(s => s.PrecioMaximo).HasPrecision(10, 2);

            // Foreign key relationship
            entity.HasOne(s => s.Vendor)
                .WithMany()
                .HasForeignKey(s => s.VendorId)
                .OnDelete(DeleteBehavior.Restrict);

        });

        modelBuilder.Entity<ServiceTag>(entity =>
        {
            entity.HasKey(st => new { st.ServiceId, st.TagId });

            entity.HasOne(st => st.Service)
                .WithMany(s => s.ServiceTags)
                .HasForeignKey(st => st.ServiceId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(st => st.Tag)
                .WithMany(t => t.ServiceTags)
                .HasForeignKey(st => st.TagId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Tag>(entity =>
        {
            entity.HasKey(t => t.Id);
            entity.Property(t => t.Nombre).IsRequired().HasMaxLength(50);
            entity.HasIndex(t => t.Nombre).IsUnique();
        });

        SeedData(modelBuilder);
    }

    private static void SeedData(ModelBuilder modelBuilder)
    {
        var adminId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var now = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        modelBuilder.Entity<User>().HasData(new User
        {
            Id = adminId,
            Nombre = "Admin BookIt",
            Telefono = "000-000-0000",
            Email = "admin@bookit.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin1234!"),
            Rol = "administrador",
            Activo = true,
            FechaCreacion = now,
            FechaActualizacion = now
        });

        var tags = new[]
        {
            new Tag { Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), Nombre = "Hogar", FechaCreacion = now },
            new Tag { Id = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), Nombre = "Tecnología", FechaCreacion = now },
            new Tag { Id = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc"), Nombre = "Salud", FechaCreacion = now },
            new Tag { Id = Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd"), Nombre = "Educación", FechaCreacion = now },
            new Tag { Id = Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee"), Nombre = "Limpieza", FechaCreacion = now },
            new Tag { Id = Guid.Parse("ffffffff-ffff-ffff-ffff-ffffffffffff"), Nombre = "Reparación", FechaCreacion = now }
        };

        modelBuilder.Entity<Tag>().HasData(tags);
    }
}
