namespace BookIt.API.Models;

public class ServiceTag
{
    public Guid ServiceId { get; set; }
    public Guid TagId { get; set; }

    // Navigation properties
    public Service? Service { get; set; }
    public Tag? Tag { get; set; }
}
