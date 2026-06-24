// BookIt-backend/BookIt.API/DTOs/VisitaDto.cs
namespace BookIt.API.DTOs;

public class VisitaDto
{
    public Guid Id { get; set; }
    public Guid ServiceId { get; set; }
    public string? ServiceNombre { get; set; }
    public Guid UserId { get; set; }
    public string? UserNombre { get; set; }
    public string? UserEmail { get; set; }
    public DateTime FechaHoraSolicitada { get; set; }
    public string Estado { get; set; } = string.Empty;
    public string? Mensaje { get; set; }
    public DateTime FechaCreacion { get; set; }
    public string? VendorNombre { get; set; }
    public string? VendorEmail { get; set; }
    public string? VendorTelefono { get; set; }
}
