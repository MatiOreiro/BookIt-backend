namespace BookIt.API.DTOs;

public class PropuestaDto
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public DateTime FechaCreacion { get; set; }
    public PropuestaItemDto Salon { get; set; } = new();
    public List<PropuestaItemDto> Servicios { get; set; } = new();
    public decimal TotalEstimado { get; set; }
}
