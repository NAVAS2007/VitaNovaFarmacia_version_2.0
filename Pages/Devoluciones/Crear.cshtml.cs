using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using VitaNovaFarmacia.Data;
using VitaNovaFarmacia.Models;

namespace VitaNovaFarmacia.Pages.Devoluciones;

[Authorize(Roles = "Administrador,Cajero")]
public class CrearModel : PageModel
{
    private readonly FarmaciaDbContext _context;

    public CrearModel(FarmaciaDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public int VentaID { get; set; }

    [BindProperty]
    public string Motivo { get; set; } = string.Empty;

    [BindProperty]
    public Dictionary<int, int> Cantidades { get; set; } = new();

    public async Task OnGetAsync() { }

    public async Task<IActionResult> OnPostAsync()
    {
        var venta = await _context.Ventas
            .Include(v => v.DetalleVentas)
            .ThenInclude(d => d.Producto)
            .FirstOrDefaultAsync(v => v.ID == VentaID);

        if (venta == null)
        {
            TempData["Error"] = "Venta no encontrada";
            return RedirectToPage();
        }

        var empleadoID = int.Parse(User.FindFirst("EmpleadoID")?.Value ?? "1");

        var devolucion = new Devolucion
        {
            VentaID = VentaID,
            EmpleadoID = empleadoID,
            Motivo = Motivo,
            Estado = "Pendiente",
            TotalDevuelto = 0
        };

        foreach (var detalle in venta.DetalleVentas)
        {
            if (Cantidades.ContainsKey(detalle.ID) && Cantidades[detalle.ID] > 0)
            {
                devolucion.Detalles.Add(new DevolucionDetalle
                {
                    DetalleVentaID = detalle.ID,
                    Cantidad = Cantidades[detalle.ID],
                    PrecioDevuelto = detalle.PrecioUnitario
                });
                devolucion.TotalDevuelto += Cantidades[detalle.ID] * detalle.PrecioUnitario;
            }
        }

        _context.Devoluciones.Add(devolucion);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Solicitud de devolucion creada. Espere autorizacion del Regente.";
        return RedirectToPage("./Index");
    }

    public async Task<IActionResult> OnGetBuscarVentaAsync(int id)
    {
        var venta = await _context.Ventas
            .Include(v => v.Cliente)
            .Include(v => v.DetalleVentas)
            .ThenInclude(d => d.Producto)
            .FirstOrDefaultAsync(v => v.ID == id);

        if (venta == null)
        {
            return new JsonResult(new { error = "Venta no encontrada" });
        }

        return new JsonResult(new
        {
            id = venta.ID,
            cliente = venta.Cliente?.Nombre ?? "Sin cliente",
            total = venta.Total,
            detalles = venta.DetalleVentas.Select(d => new
            {
                id = d.ID,
                producto = d.Producto?.Nombre ?? "N/A",
                cantidad = d.Cantidad,
                precio = d.PrecioUnitario
            })
        });
    }
}