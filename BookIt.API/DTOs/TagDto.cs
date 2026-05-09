namespace BookIt.API.DTOs;

public class TagDto
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public DateTime FechaCreacion { get; set; }
}

public class ServiceTagDto
{
    public Guid Id { get; set; }
    public Guid VendorId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public string Ubicacion { get; set; } = string.Empty;
    public string TipoServicio { get; set; } = string.Empty;
    public decimal PrecioMinimo { get; set; }
    public decimal PrecioMaximo { get; set; }
    public bool Activo { get; set; }
    public DateTime FechaCreacion { get; set; }
    public DateTime FechaActualizacion { get; set; }
    public VendorDto? Vendor { get; set; }
    public List<TagDto> Tags { get; set; } = new();
}
