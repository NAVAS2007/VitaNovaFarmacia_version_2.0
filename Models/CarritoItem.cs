namespace VitaNovaFarmacia.Models;

public class CarritoItem
{
    public int ProductoID { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? ComponenteActivo { get; set; }
    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal Subtotal => Cantidad * PrecioUnitario;
    public int? LoteIDSeleccionado { get; set; }
}