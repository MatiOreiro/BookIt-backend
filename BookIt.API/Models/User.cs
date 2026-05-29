namespace BookIt.API.Models;

public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Nombre { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? ProfileImageUrl { get; set; }
    public string PasswordHash { get; set; } = string.Empty;
    public string Rol { get; set; } = "usuario";
    public bool Activo { get; set; } = true;
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    public DateTime FechaActualizacion { get; set; } = DateTime.UtcNow;

    public ICollection<Reserva> Reservas { get; set; } = new List<Reserva>();
    public ICollection<Visita> Visitas { get; set; } = new List<Visita>();
}
