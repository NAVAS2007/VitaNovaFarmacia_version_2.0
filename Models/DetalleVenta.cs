using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VitaNovaFarmacia.Models;

public class DetalleVenta
{
    [Key]
    public int ID { get; set; }

    public int VentaID { get; set; }

    public int ProductoID { get; set; }

    public int LoteID { get; set; }

    public int Cantidad { get; set; }

    public decimal PrecioUnitario { get; set; }

    public decimal Subtotal => Cantidad * PrecioUnitario;

    [ForeignKey(nameof(VentaID))]
    public Venta? Venta { get; set; }

    [ForeignKey(nameof(ProductoID))]
    public Producto? Producto { get; set; }

    [ForeignKey(nameof(LoteID))]
    public Lote? Lote { get; set; }
}