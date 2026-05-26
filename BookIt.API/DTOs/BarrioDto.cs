namespace BookIt.API.DTOs;

public class BarrioDto
{
    public Guid Id { get; set; }
    public Guid DepartamentoId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public DepartamentoDto? Departamento { get; set; }
}