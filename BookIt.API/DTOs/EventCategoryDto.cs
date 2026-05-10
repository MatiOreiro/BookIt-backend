namespace BookIt.API.DTOs;

public class EventCategoryDto
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public DateTime FechaCreacion { get; set; }
}