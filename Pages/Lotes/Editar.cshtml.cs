using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using VitaNovaFarmacia.Data;
using VitaNovaFarmacia.Models;

namespace VitaNovaFarmacia.Pages.Lotes;

[Authorize(Roles = "Administrador,Regente")]
public class EditarModel : PageModel
{
    private readonly FarmaciaDbContext _context;

    public EditarModel(FarmaciaDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public Lote Lote { get; set; } = new();

    public SelectList ListaProductos { get; set; } = null!;
    public SelectList ListaProveedores { get; set; } = null!;

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var lote = await _context.Lotes
            .FirstOrDefaultAsync(l => l.ID == id);

        if (lote == null)
        {
            return NotFound();
        }

        Lote = lote;
        await CargarDatos();

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            await CargarDatos();
            return Page();
        }

        var loteBD = await _context.Lotes
            .FirstOrDefaultAsync(l => l.ID == Lote.ID);

        if (loteBD == null)
        {
            return NotFound();
        }

        var valorAnterior = $"Lote: {loteBD.NumeroLote}, Cantidad: {loteBD.Cantidad}, Vencimiento: {loteBD.FechaVencimiento:dd/MM/yyyy}";

        loteBD.ProductoID = Lote.ProductoID;
        loteBD.NumeroLote = Lote.NumeroLote;
        loteBD.Cantidad = Lote.Cantidad;
        loteBD.CostoAdquisicion = Lote.CostoAdquisicion;
        loteBD.ProveedorID = Lote.ProveedorID;
        loteBD.FechaVencimiento = Lote.FechaVencimiento;
        loteBD.Activo = Lote.Activo;

        await _context.SaveChangesAsync();

        await RegistrarBitacora("EDICION", "Lotes", valorAnterior, $"Lote: {loteBD.NumeroLote}, Cantidad: {loteBD.Cantidad}, Vencimiento: {loteBD.FechaVencimiento:dd/MM/yyyy}");

        TempData["Success"] = $"Lote {loteBD.NumeroLote} actualizado exitosamente.";
        return RedirectToPage("./Index");
    }

    private async Task CargarDatos()
    {
        var productos = await _context.Productos
            .Where(p => p.Activo)
            .OrderBy(p => p.Nombre)
            .ToListAsync();

        var proveedores = await _context.Proveedores
            .Where(p => p.Activo)
            .OrderBy(p => p.Nombre)
            .ToListAsync();

        ListaProductos = new SelectList(productos, "ID", "Nombre");
        ListaProveedores = new SelectList(proveedores, "ID", "Nombre");
    }

    private async Task RegistrarBitacora(string accion, string tabla, string valorAnterior, string valorNuevo)
    {
        var empleadoID = int.Parse(User.FindFirst("EmpleadoID")?.Value ?? "1");

        var bitacora = new BitacoraAuditoria
        {
            EmpleadoID = empleadoID,
            AccionEjecutada = accion,
            TablaAfectada = tabla,
            ValorAnterior = valorAnterior,
            ValorNuevo = valorNuevo
        };
        _context.BitacoraAuditoria.Add(bitacora);
        await _context.SaveChangesAsync();
    }
}