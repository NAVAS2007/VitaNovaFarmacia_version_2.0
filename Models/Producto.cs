using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VitaNovaFarmacia.Models;

public class Producto
{
    [Key]
    public int ID { get; set; }

    [StringLength(100)]
    public string? CodigoBarras { get; set; }

    [Required]
    [StringLength(150)]
    public string Nombre { get; set; } = string.Empty;

    [StringLength(150)]
    public string? ComponenteActivo { get; set; }

    [Required]
    public decimal Precio { get; set; }

    public int StockMinimo { get; set; } = 10;

    public int? IdCategoria { get; set; }

    public bool Activo { get; set; } = true;

    [StringLength(200)]
    public string? Imagen { get; set; }

    [StringLength(250)]
    public string? Descripcion { get; set; }

    public bool RequiereReceta { get; set; }

    [ForeignKey(nameof(IdCategoria))]
    public Categoria? Categoria { get; set; }

    public ICollection<Lote> Lotes { get; set; } = new List<Lote>();

    public ICollection<DetalleVenta> DetalleVentas { get; set; } = new List<DetalleVenta>();
}