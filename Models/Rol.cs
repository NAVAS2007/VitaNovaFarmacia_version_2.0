using System.ComponentModel.DataAnnotations;

namespace VitaNovaFarmacia.Models;

public class Rol
{
    [Key]
    public int ID { get; set; }

    [Required]
    [StringLength(50)]
    public string NombreRole { get; set; } = string.Empty;

    public ICollection<Empleado> Empleados { get; set; } = new List<Empleado>();
}