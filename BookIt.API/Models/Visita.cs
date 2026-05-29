namespace BookIt.API.Models;

public class Visita
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ServiceId { get; set; }
    public Guid UserId { get; set; }
    public DateTime FechaHoraSolicitada { get; set; }
    public string Estado { get; set; } = "Pendiente";
    public string? Mensaje { get; set; }
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    public DateTime? FechaActualizacion { get; set; } = DateTime.UtcNow;

    public Service? Service { get; set; }
    public User? User { get; set; }
}
