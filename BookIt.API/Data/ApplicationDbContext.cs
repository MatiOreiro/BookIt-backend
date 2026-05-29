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
    public DbSet<EventCategory> EventCategories { get; set; }
    public DbSet<ServiceEventCategory> ServiceEventCategories { get; set; }
    public DbSet<Tag> Tags { get; set; }
    public DbSet<ServiceTag> ServiceTags { get; set; }
    public DbSet<Departamento> Departamentos { get; set; }
    public DbSet<Barrio> Barrios { get; set; }
    public DbSet<Direccion> Direcciones { get; set; }
    public DbSet<Reserva> Reservas { get; set; }
    public DbSet<Visita> Visitas { get; set; }

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
            entity.Property(u => u.ProfileImageUrl).HasMaxLength(400);
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
            entity.Property(s => s.ImageUrlsJson).HasColumnType("text");

            // Foreign key relationship
            entity.HasOne(s => s.Vendor)
                .WithMany()
                .HasForeignKey(s => s.VendorId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(s => s.DireccionCompleta)
                .WithMany(d => d.Services)
                .HasForeignKey(s => s.DireccionId)
                .OnDelete(DeleteBehavior.Restrict);

        });

        modelBuilder.Entity<Departamento>(entity =>
        {
            entity.HasKey(d => d.Id);
            entity.Property(d => d.Nombre).IsRequired().HasMaxLength(100);
            entity.HasIndex(d => d.Nombre).IsUnique();
        });

        modelBuilder.Entity<Barrio>(entity =>
        {
            entity.HasKey(b => b.Id);
            entity.Property(b => b.Nombre).IsRequired().HasMaxLength(120);
            entity.HasIndex(b => new { b.DepartamentoId, b.Nombre }).IsUnique();

            entity.HasOne(b => b.Departamento)
                .WithMany(d => d.Barrios)
                .HasForeignKey(b => b.DepartamentoId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Direccion>(entity =>
        {
            entity.HasKey(d => d.Id);
            entity.Property(d => d.Calle).IsRequired().HasMaxLength(200);

            entity.HasOne(d => d.Departamento)
                .WithMany()
                .HasForeignKey(d => d.DepartamentoId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(d => d.Barrio)
                .WithMany(b => b.Direcciones)
                .HasForeignKey(d => d.BarrioId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Reserva>(entity =>
        {
            entity.HasKey(r => r.Id);
            entity.Property(r => r.Confirmada).HasDefaultValue(false);
            entity.Property(r => r.FechaReservaCliente).IsRequired();

            entity.HasOne(r => r.Service)
                .WithMany(s => s.Reservas)
                .HasForeignKey(r => r.ServiceId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(r => r.User)
                .WithMany(u => u.Reservas)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Visita>(entity =>
        {
            entity.HasKey(v => v.Id);
            entity.Property(v => v.Estado).IsRequired().HasMaxLength(30);
            entity.Property(v => v.Mensaje).HasMaxLength(500);
            entity.Property(v => v.FechaHoraSolicitada).IsRequired();
            entity.Property(v => v.FechaCreacion).IsRequired();

            entity.HasOne(v => v.Service)
                .WithMany(s => s.Visitas)
                .HasForeignKey(v => v.ServiceId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(v => v.User)
                .WithMany(u => u.Visitas)
                .HasForeignKey(v => v.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(v => new { v.ServiceId, v.FechaHoraSolicitada });
            entity.HasIndex(v => new { v.ServiceId, v.FechaHoraSolicitada, v.Estado });
        });

        modelBuilder.Entity<ServiceEventCategory>(entity =>
        {
            entity.HasKey(sc => new { sc.ServiceId, sc.EventCategoryId });

            entity.HasOne(sc => sc.Service)
                .WithMany(s => s.ServiceEventCategories)
                .HasForeignKey(sc => sc.ServiceId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(sc => sc.EventCategory)
                .WithMany(c => c.ServiceEventCategories)
                .HasForeignKey(sc => sc.EventCategoryId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<EventCategory>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Nombre).IsRequired().HasMaxLength(50);
            entity.HasIndex(c => c.Nombre).IsUnique();
        });

        modelBuilder.Entity<Tag>(entity =>
        {
            entity.HasKey(t => t.Id);
            entity.Property(t => t.Nombre).IsRequired().HasMaxLength(100);
            entity.HasIndex(t => t.Nombre).IsUnique();
        });

        modelBuilder.Entity<ServiceTag>(entity =>
        {
            entity.HasKey(st => new { st.ServiceId, st.TagId });

            entity.HasOne(st => st.Service)
                .WithMany()
                .HasForeignKey(st => st.ServiceId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(st => st.Tag)
                .WithMany(t => t.ServiceTags)
                .HasForeignKey(st => st.TagId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        SeedGeography(modelBuilder);
        SeedData(modelBuilder);
    }

    private static void SeedGeography(ModelBuilder modelBuilder)
    {
        var departamentos = new[]
        {
            new Departamento { Id = Guid.Parse("10000000-0000-0000-0000-000000000001"), Nombre = "Artigas" },
            new Departamento { Id = Guid.Parse("10000000-0000-0000-0000-000000000002"), Nombre = "Canelones" },
            new Departamento { Id = Guid.Parse("10000000-0000-0000-0000-000000000003"), Nombre = "Cerro Largo" },
            new Departamento { Id = Guid.Parse("10000000-0000-0000-0000-000000000004"), Nombre = "Colonia" },
            new Departamento { Id = Guid.Parse("10000000-0000-0000-0000-000000000005"), Nombre = "Durazno" },
            new Departamento { Id = Guid.Parse("10000000-0000-0000-0000-000000000006"), Nombre = "Flores" },
            new Departamento { Id = Guid.Parse("10000000-0000-0000-0000-000000000007"), Nombre = "Florida" },
            new Departamento { Id = Guid.Parse("10000000-0000-0000-0000-000000000008"), Nombre = "Lavalleja" },
            new Departamento { Id = Guid.Parse("10000000-0000-0000-0000-000000000009"), Nombre = "Maldonado" },
            new Departamento { Id = Guid.Parse("10000000-0000-0000-0000-000000000010"), Nombre = "Montevideo" },
            new Departamento { Id = Guid.Parse("10000000-0000-0000-0000-000000000011"), Nombre = "Paysandú" },
            new Departamento { Id = Guid.Parse("10000000-0000-0000-0000-000000000012"), Nombre = "Río Negro" },
            new Departamento { Id = Guid.Parse("10000000-0000-0000-0000-000000000013"), Nombre = "Rivera" },
            new Departamento { Id = Guid.Parse("10000000-0000-0000-0000-000000000014"), Nombre = "Rocha" },
            new Departamento { Id = Guid.Parse("10000000-0000-0000-0000-000000000015"), Nombre = "Salto" },
            new Departamento { Id = Guid.Parse("10000000-0000-0000-0000-000000000016"), Nombre = "San José" },
            new Departamento { Id = Guid.Parse("10000000-0000-0000-0000-000000000017"), Nombre = "Soriano" },
            new Departamento { Id = Guid.Parse("10000000-0000-0000-0000-000000000018"), Nombre = "Tacuarembó" },
            new Departamento { Id = Guid.Parse("10000000-0000-0000-0000-000000000019"), Nombre = "Treinta y Tres" }
        };

        modelBuilder.Entity<Departamento>().HasData(departamentos);

        var montevideoId = departamentos.Single(d => d.Nombre == "Montevideo").Id;
        var canelonesId = departamentos.Single(d => d.Nombre == "Canelones").Id;
        var maldonadoId = departamentos.Single(d => d.Nombre == "Maldonado").Id;

        var barrios = new[]
        {
            new Barrio { Id = Guid.Parse("20000000-0000-0000-0000-000000000001"), DepartamentoId = montevideoId, Nombre = "Centro" },
            new Barrio { Id = Guid.Parse("20000000-0000-0000-0000-000000000002"), DepartamentoId = montevideoId, Nombre = "Cordón" },
            new Barrio { Id = Guid.Parse("20000000-0000-0000-0000-000000000003"), DepartamentoId = montevideoId, Nombre = "Ciudad Vieja" },
            new Barrio { Id = Guid.Parse("20000000-0000-0000-0000-000000000004"), DepartamentoId = montevideoId, Nombre = "Palermo" },
            new Barrio { Id = Guid.Parse("20000000-0000-0000-0000-000000000005"), DepartamentoId = montevideoId, Nombre = "Parque Rodó" },
            new Barrio { Id = Guid.Parse("20000000-0000-0000-0000-000000000006"), DepartamentoId = montevideoId, Nombre = "Pocitos" },
            new Barrio { Id = Guid.Parse("20000000-0000-0000-0000-000000000007"), DepartamentoId = montevideoId, Nombre = "Punta Carretas" },
            new Barrio { Id = Guid.Parse("20000000-0000-0000-0000-000000000008"), DepartamentoId = montevideoId, Nombre = "Malvín" },
            new Barrio { Id = Guid.Parse("20000000-0000-0000-0000-000000000009"), DepartamentoId = montevideoId, Nombre = "Buceo" },
            new Barrio { Id = Guid.Parse("20000000-0000-0000-0000-000000000010"), DepartamentoId = montevideoId, Nombre = "Punta Gorda" },
            new Barrio { Id = Guid.Parse("20000000-0000-0000-0000-000000000011"), DepartamentoId = montevideoId, Nombre = "Carrasco" },
            new Barrio { Id = Guid.Parse("20000000-0000-0000-0000-000000000012"), DepartamentoId = montevideoId, Nombre = "Cerro" },
            new Barrio { Id = Guid.Parse("20000000-0000-0000-0000-000000000013"), DepartamentoId = montevideoId, Nombre = "Aguada" },
            new Barrio { Id = Guid.Parse("20000000-0000-0000-0000-000000000014"), DepartamentoId = montevideoId, Nombre = "Reducto" },
            new Barrio { Id = Guid.Parse("20000000-0000-0000-0000-000000000015"), DepartamentoId = montevideoId, Nombre = "La Blanqueada" },
            new Barrio { Id = Guid.Parse("20000000-0000-0000-0000-000000000016"), DepartamentoId = montevideoId, Nombre = "Prado" },
            new Barrio { Id = Guid.Parse("20000000-0000-0000-0000-000000000017"), DepartamentoId = montevideoId, Nombre = "Tajamar" },
            new Barrio { Id = Guid.Parse("20000000-0000-0000-0000-000000000018"), DepartamentoId = montevideoId, Nombre = "La Comercial" },
            new Barrio { Id = Guid.Parse("20000000-0000-0000-0000-000000000019"), DepartamentoId = montevideoId, Nombre = "Brazo Oriental" },
            new Barrio { Id = Guid.Parse("20000000-0000-0000-0000-000000000020"), DepartamentoId = montevideoId, Nombre = "Belvedere" },
            new Barrio { Id = Guid.Parse("20000000-0000-0000-0000-000000000021"), DepartamentoId = montevideoId, Nombre = "Capurro" },
            new Barrio { Id = Guid.Parse("20000000-0000-0000-0000-000000000022"), DepartamentoId = montevideoId, Nombre = "Jacinto Vera" },
            new Barrio { Id = Guid.Parse("20000000-0000-0000-0000-000000000023"), DepartamentoId = montevideoId, Nombre = "Paso de las Duranas" },
            new Barrio { Id = Guid.Parse("20000000-0000-0000-0000-000000000024"), DepartamentoId = montevideoId, Nombre = "Piedras Blancas" },
            new Barrio { Id = Guid.Parse("20000000-0000-0000-0000-000000000025"), DepartamentoId = montevideoId, Nombre = "Casavalle" },

            new Barrio { Id = Guid.Parse("20000000-0000-0000-0000-000000000101"), DepartamentoId = canelonesId, Nombre = "Ciudad de la Costa" },
            new Barrio { Id = Guid.Parse("20000000-0000-0000-0000-000000000102"), DepartamentoId = canelonesId, Nombre = "Las Piedras" },
            new Barrio { Id = Guid.Parse("20000000-0000-0000-0000-000000000103"), DepartamentoId = canelonesId, Nombre = "Pando" },
            new Barrio { Id = Guid.Parse("20000000-0000-0000-0000-000000000104"), DepartamentoId = canelonesId, Nombre = "Barros Blancos" },
            new Barrio { Id = Guid.Parse("20000000-0000-0000-0000-000000000105"), DepartamentoId = canelonesId, Nombre = "La Paz" },
            new Barrio { Id = Guid.Parse("20000000-0000-0000-0000-000000000106"), DepartamentoId = canelonesId, Nombre = "Santa Lucía" },

            new Barrio { Id = Guid.Parse("20000000-0000-0000-0000-000000000201"), DepartamentoId = maldonadoId, Nombre = "Maldonado" },
            new Barrio { Id = Guid.Parse("20000000-0000-0000-0000-000000000202"), DepartamentoId = maldonadoId, Nombre = "Punta del Este" },
            new Barrio { Id = Guid.Parse("20000000-0000-0000-0000-000000000203"), DepartamentoId = maldonadoId, Nombre = "San Carlos" },
            new Barrio { Id = Guid.Parse("20000000-0000-0000-0000-000000000204"), DepartamentoId = maldonadoId, Nombre = "Piriápolis" },
            new Barrio { Id = Guid.Parse("20000000-0000-0000-0000-000000000205"), DepartamentoId = maldonadoId, Nombre = "La Barra" }
        };

        var remainingDepartamentoBarrios = new[]
        {
            new Barrio { Id = Guid.Parse("20000000-0000-0000-0000-000000000301"), DepartamentoId = departamentos.Single(d => d.Nombre == "Artigas").Id, Nombre = "Artigas" },
            new Barrio { Id = Guid.Parse("20000000-0000-0000-0000-000000000302"), DepartamentoId = departamentos.Single(d => d.Nombre == "Cerro Largo").Id, Nombre = "Cerro Largo" },
            new Barrio { Id = Guid.Parse("20000000-0000-0000-0000-000000000303"), DepartamentoId = departamentos.Single(d => d.Nombre == "Colonia").Id, Nombre = "Colonia" },
            new Barrio { Id = Guid.Parse("20000000-0000-0000-0000-000000000304"), DepartamentoId = departamentos.Single(d => d.Nombre == "Durazno").Id, Nombre = "Durazno" },
            new Barrio { Id = Guid.Parse("20000000-0000-0000-0000-000000000305"), DepartamentoId = departamentos.Single(d => d.Nombre == "Flores").Id, Nombre = "Flores" },
            new Barrio { Id = Guid.Parse("20000000-0000-0000-0000-000000000306"), DepartamentoId = departamentos.Single(d => d.Nombre == "Florida").Id, Nombre = "Florida" },
            new Barrio { Id = Guid.Parse("20000000-0000-0000-0000-000000000307"), DepartamentoId = departamentos.Single(d => d.Nombre == "Lavalleja").Id, Nombre = "Lavalleja" },
            new Barrio { Id = Guid.Parse("20000000-0000-0000-0000-000000000308"), DepartamentoId = departamentos.Single(d => d.Nombre == "Paysandú").Id, Nombre = "Paysandú" },
            new Barrio { Id = Guid.Parse("20000000-0000-0000-0000-000000000309"), DepartamentoId = departamentos.Single(d => d.Nombre == "Río Negro").Id, Nombre = "Río Negro" },
            new Barrio { Id = Guid.Parse("20000000-0000-0000-0000-000000000310"), DepartamentoId = departamentos.Single(d => d.Nombre == "Rivera").Id, Nombre = "Rivera" },
            new Barrio { Id = Guid.Parse("20000000-0000-0000-0000-000000000311"), DepartamentoId = departamentos.Single(d => d.Nombre == "Rocha").Id, Nombre = "Rocha" },
            new Barrio { Id = Guid.Parse("20000000-0000-0000-0000-000000000312"), DepartamentoId = departamentos.Single(d => d.Nombre == "Salto").Id, Nombre = "Salto" },
            new Barrio { Id = Guid.Parse("20000000-0000-0000-0000-000000000313"), DepartamentoId = departamentos.Single(d => d.Nombre == "San José").Id, Nombre = "San José" },
            new Barrio { Id = Guid.Parse("20000000-0000-0000-0000-000000000314"), DepartamentoId = departamentos.Single(d => d.Nombre == "Soriano").Id, Nombre = "Soriano" },
            new Barrio { Id = Guid.Parse("20000000-0000-0000-0000-000000000315"), DepartamentoId = departamentos.Single(d => d.Nombre == "Tacuarembó").Id, Nombre = "Tacuarembó" },
            new Barrio { Id = Guid.Parse("20000000-0000-0000-0000-000000000316"), DepartamentoId = departamentos.Single(d => d.Nombre == "Treinta y Tres").Id, Nombre = "Treinta y Tres" }
        };

        barrios = barrios.Concat(remainingDepartamentoBarrios).ToArray();

        modelBuilder.Entity<Barrio>().HasData(barrios);
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

        var categories = new[]
        {
            new EventCategory { Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), Nombre = "Boda", FechaCreacion = now },
            new EventCategory { Id = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), Nombre = "Cumpleaños de XV", FechaCreacion = now },
            new EventCategory { Id = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc"), Nombre = "Cumpleaños", FechaCreacion = now },
            new EventCategory { Id = Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd"), Nombre = "Evento corporativo", FechaCreacion = now },
            new EventCategory { Id = Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee"), Nombre = "Bautismo", FechaCreacion = now },
            new EventCategory { Id = Guid.Parse("ffffffff-ffff-ffff-ffff-ffffffffffff"), Nombre = "Graduación", FechaCreacion = now },
            new EventCategory { Id = Guid.Parse("99999999-9999-9999-9999-999999999999"), Nombre = "Baile", FechaCreacion = now }
        };

        modelBuilder.Entity<EventCategory>().HasData(categories);
    }
}
