namespace BookIt.API.DTOs;

public class DireccionDto
{
    public Guid Id { get; set; }
    public DepartamentoDto? Departamento { get; set; }
    public BarrioDto? Barrio { get; set; }
    public string Calle { get; set; } = string.Empty;
}