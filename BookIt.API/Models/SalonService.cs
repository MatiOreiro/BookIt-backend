namespace BookIt.API.Models;

public class SalonService
{
    public Guid SalonId { get; set; }
    public Guid ServiceId { get; set; }
    public Service? Salon { get; set; }
    public Service? Servicio { get; set; }
}
