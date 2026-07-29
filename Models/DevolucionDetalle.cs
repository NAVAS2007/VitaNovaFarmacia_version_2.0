using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VitaNovaFarmacia.Models;

public class DevolucionDetalle
{
    [Key]
    public int ID { get; set; }

    public int DevolucionID { get; set; }

    public int DetalleVentaID { get; set; }

    public int Cantidad { get; set; }

    public decimal PrecioDevuelto { get; set; }

    [ForeignKey(nameof(DevolucionID))]
    public Devolucion? Devolucion { get; set; }

    [ForeignKey(nameof(DetalleVentaID))]
    public DetalleVenta? DetalleVenta { get; set; }
}