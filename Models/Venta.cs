using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VitaNovaFarmacia.Models;

public class Venta
{
    [Key]
    public int ID { get; set; }

    public DateTime Fecha { get; set; } = DateTime.Now;

    public int ClienteID { get; set; }

    public int EmpleadoID { get; set; }

    public int MetodoPagoID { get; set; }

    public decimal Total { get; set; }

    public decimal? Recibido { get; set; }

    public decimal? Cambio { get; set; }

    public string Estado { get; set; } = "Completada";

    [ForeignKey(nameof(ClienteID))]
    public Cliente? Cliente { get; set; }

    [ForeignKey(nameof(EmpleadoID))]
    public Empleado? Empleado { get; set; }

    [ForeignKey(nameof(MetodoPagoID))]
    public MetodoPago? MetodoPago { get; set; }

    public ICollection<DetalleVenta> DetalleVentas { get; set; } = new List<DetalleVenta>();
}