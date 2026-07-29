using System.ComponentModel.DataAnnotations;

namespace VitaNovaFarmacia.Models;

public class MetodoPago
{
    [Key]
    public int ID { get; set; }

    [Required]
    [StringLength(50)]
    public string Nombre { get; set; } = string.Empty;

    public ICollection<Venta> Ventas { get; set; } = new List<Venta>();
}