namespace BookIt.API.Models;

public class Resena
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ReservaId { get; set; }
    public Guid ServiceId { get; set; }
    public Guid UserId { get; set; }
    public int Puntuacion { get; set; }
    public string? Comentario { get; set; }
    public string? MediaUrlsJson { get; set; }
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    public DateTime FechaActualizacion { get; set; } = DateTime.UtcNow;

    public Reserva? Reserva { get; set; }
    public Service? Service { get; set; }
    public User? User { get; set; }
}
