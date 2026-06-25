namespace BookIt.API.DTOs;

public class ServicioAsociadoDto
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string TipoServicio { get; set; } = string.Empty;
    public decimal PrecioMinimo { get; set; }
    public decimal PrecioMaximo { get; set; }
    public string Descripcion { get; set; } = string.Empty;
}
