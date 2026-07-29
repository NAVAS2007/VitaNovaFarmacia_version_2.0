using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VitaNovaFarmacia.Models;

public class Empleado
{
    [Key]
    public int ID { get; set; }

    [Required]
    [StringLength(100)]
    public string Nombre { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(100)]
    public string Email { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public int RolID { get; set; }

    public bool Activo { get; set; } = true;

    public DateTime FechaIngreso { get; set; } = DateTime.Now;

    public DateTime? UltimoLogin { get; set; }

    [ForeignKey(nameof(RolID))]
    public Rol? Rol { get; set; }

    public ICollection<Venta> Ventas { get; set; } = new List<Venta>();
}