using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VitaNovaFarmacia.Models;

public class Devolucion
{
    [Key]
    public int ID { get; set; }

    public int VentaID { get; set; }

    public int EmpleadoID { get; set; }

    public int? RegenteID { get; set; }

    public DateTime Fecha { get; set; } = DateTime.Now;

    [StringLength(250)]
    public string Motivo { get; set; } = string.Empty;

    [StringLength(50)]
    public string Estado { get; set; } = "Pendiente";

    public decimal TotalDevuelto { get; set; }

    [StringLength(255)]
    public string? CredencialRegente { get; set; }

    [ForeignKey(nameof(VentaID))]
    public Venta? Venta { get; set; }

    [ForeignKey(nameof(EmpleadoID))]
    public Empleado? Empleado { get; set; }

    [ForeignKey(nameof(RegenteID))]
    public Empleado? Regente { get; set; }

    public ICollection<DevolucionDetalle> Detalles { get; set; } = new List<DevolucionDetalle>();
}