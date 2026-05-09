namespace BookIt.API.Models;

public class Tag
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Nombre { get; set; } = string.Empty;
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    // Navigation property
    public ICollection<ServiceTag> ServiceTags { get; set; } = new List<ServiceTag>();
}
