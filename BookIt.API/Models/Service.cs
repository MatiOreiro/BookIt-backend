namespace BookIt.API.Models;

public class Service
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid VendorId { get; set; } // FK to User (vendor)
    public string Nombre { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public string Ubicacion { get; set; } = string.Empty;
    public string TipoServicio { get; set; } = string.Empty;
    public decimal PrecioMinimo { get; set; }
    public decimal PrecioMaximo { get; set; }
    public bool Activo { get; set; } = true;
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    public DateTime FechaActualizacion { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public User? Vendor { get; set; }
    public ICollection<ServiceTag> ServiceTags { get; set; } = new List<ServiceTag>();
}
