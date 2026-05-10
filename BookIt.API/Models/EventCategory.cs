namespace BookIt.API.Models;

public class EventCategory
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Nombre { get; set; } = string.Empty;
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    public ICollection<ServiceEventCategory> ServiceEventCategories { get; set; } = new List<ServiceEventCategory>();
}