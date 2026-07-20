namespace BookIt.API.DTOs;

public class PropuestaItemDto
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string TipoServicio { get; set; } = string.Empty;
    public decimal PrecioMinimo { get; set; }
    public string? VendorNombre { get; set; }
    public string? VendorEmail { get; set; }
    public string? VendorTelefono { get; set; }
}
