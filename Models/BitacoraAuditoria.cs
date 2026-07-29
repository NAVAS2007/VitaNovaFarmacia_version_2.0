using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VitaNovaFarmacia.Models;

public class BitacoraAuditoria
{
    [Key]
    public int ID { get; set; }

    public DateTime FechaHora { get; set; } = DateTime.Now;

    public int EmpleadoID { get; set; }

    [StringLength(50)]
    public string AccionEjecutada { get; set; } = string.Empty;

    [StringLength(50)]
    public string TablaAfectada { get; set; } = string.Empty;

    public string? ValorAnterior { get; set; }

    public string? ValorNuevo { get; set; }

    [ForeignKey(nameof(EmpleadoID))]
    public Empleado? Empleado { get; set; }
}