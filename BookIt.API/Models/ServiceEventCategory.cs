namespace BookIt.API.Models;

public class ServiceEventCategory
{
    public Guid ServiceId { get; set; }
    public Guid EventCategoryId { get; set; }

    public Service? Service { get; set; }
    public EventCategory? EventCategory { get; set; }
}