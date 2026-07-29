using System.ComponentModel.DataAnnotations;

namespace VitaNovaFarmacia.Models;

public class Cliente
{
    [Key]
    public int ID { get; set; }

    [Required]
    [StringLength(20)]
    public string Documento { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string Nombre { get; set; } = string.Empty;

    [StringLength(100)]
    public string? Apellido { get; set; }

    [StringLength(20)]
    public string? Telefono { get; set; }

    [EmailAddress]
    [StringLength(100)]
    public string? Correo { get; set; }

    [StringLength(250)]
    public string? Direccion { get; set; }

    public DateTime FechaRegistro { get; set; } = DateTime.Now;

    public bool Activo { get; set; } = true;

    public ICollection<Venta> Ventas { get; set; } = new List<Venta>();
}