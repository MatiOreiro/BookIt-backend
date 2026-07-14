namespace BookIt.API.Models;

public class PropuestaServicio
{
    public Guid PropuestaId { get; set; }
    public Guid ServiceId { get; set; }
    public Propuesta? Propuesta { get; set; }
    public Service? Service { get; set; }
}
