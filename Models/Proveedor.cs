using System.ComponentModel.DataAnnotations;

namespace VitaNovaFarmacia.Models;

public class Proveedor
{
    [Key]
    public int ID { get; set; }

    [Required]
    [StringLength(150)]
    public string Nombre { get; set; } = string.Empty;

    [StringLength(20)]
    public string? Telefono { get; set; }

    [EmailAddress]
    [StringLength(100)]
    public string? Correo { get; set; }

    [StringLength(250)]
    public string? Direccion { get; set; }

    [StringLength(50)]
    public string? NIT { get; set; }

    public bool Activo { get; set; } = true;

    public DateTime FechaRegistro { get; set; } = DateTime.Now;

    public ICollection<Lote> Lotes { get; set; } = new List<Lote>();
}