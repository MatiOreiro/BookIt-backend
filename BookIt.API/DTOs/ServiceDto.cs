namespace BookIt.API.DTOs;

public class ServiceDto
{
    public Guid Id { get; set; }
    public Guid VendorId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public string Ubicacion { get; set; } = string.Empty;
    public string TipoServicio { get; set; } = string.Empty;
    public decimal PrecioMinimo { get; set; }
    public decimal PrecioMaximo { get; set; }
    public int? Capacidad { get; set; }
    public bool Activo { get; set; }
    public DateTime FechaCreacion { get; set; }
    public DateTime FechaActualizacion { get; set; }
    public VendorDto? Vendor { get; set; }
    public DireccionDto? Direccion { get; set; }
    public List<EventCategoryDto> Categorias { get; set; } = new();
    public List<ReservaDto> Reservas { get; set; } = new();
    public List<VisitaDto> Visitas { get; set; } = new();
    public List<string> Imagenes { get; set; } = new();
}

public class VendorDto
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public string? ProfileImageUrl { get; set; }
}
