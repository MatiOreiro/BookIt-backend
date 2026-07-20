namespace BookIt.API.Models;

public class Propuesta
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public Guid SalonId { get; set; }
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    public User? User { get; set; }
    public Service? Salon { get; set; }
    public ICollection<PropuestaServicio> Servicios { get; set; } = new List<PropuestaServicio>();
}
