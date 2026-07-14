namespace BookIt.API.DTOs;

public class ResenaDto
{
    public Guid Id { get; set; }
    public Guid ReservaId { get; set; }
    public Guid ServiceId { get; set; }
    public Guid UserId { get; set; }
    public int Puntuacion { get; set; }
    public string? Comentario { get; set; }
    public List<string> MediaUrls { get; set; } = [];
    public DateTime FechaCreacion { get; set; }
    public DateTime FechaActualizacion { get; set; }
    public string? UsuarioNombre { get; set; }
    public string? UsuarioProfileImageUrl { get; set; }
}
