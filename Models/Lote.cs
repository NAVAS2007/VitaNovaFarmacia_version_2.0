using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VitaNovaFarmacia.Models;

public class Lote
{
    [Key]
    public int ID { get; set; }

    public int ProductoID { get; set; }

    [StringLength(100)]
    public string NumeroLote { get; set; } = string.Empty;

    public int Cantidad { get; set; }

    public decimal CostoAdquisicion { get; set; }

    public int? ProveedorID { get; set; }

    public DateTime FechaIngreso { get; set; } = DateTime.Now;

    public DateTime FechaVencimiento { get; set; }

    public bool Activo { get; set; } = true;

    [ForeignKey(nameof(ProductoID))]
    public Producto? Producto { get; set; }

    [ForeignKey(nameof(ProveedorID))]
    public Proveedor? Proveedor { get; set; }
}