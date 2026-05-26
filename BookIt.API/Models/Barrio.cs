namespace BookIt.API.Models;

public class Barrio
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid DepartamentoId { get; set; }
    public string Nombre { get; set; } = string.Empty;

    public Departamento? Departamento { get; set; }
    public ICollection<Direccion> Direcciones { get; set; } = new List<Direccion>();
}