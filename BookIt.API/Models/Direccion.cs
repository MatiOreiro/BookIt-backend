namespace BookIt.API.Models;

public class Direccion
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid DepartamentoId { get; set; }
    public Guid BarrioId { get; set; }
    public string Calle { get; set; } = string.Empty;

    public Departamento? Departamento { get; set; }
    public Barrio? Barrio { get; set; }
    public ICollection<Service> Services { get; set; } = new List<Service>();
}