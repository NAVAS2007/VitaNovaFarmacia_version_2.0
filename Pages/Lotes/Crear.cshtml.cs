using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using VitaNovaFarmacia.Data;
using VitaNovaFarmacia.Models;

namespace VitaNovaFarmacia.Pages.Lotes;

[Authorize(Roles = "Administrador,Regente")]
public class CrearModel : PageModel
{
    private readonly FarmaciaDbContext _context;

    public CrearModel(FarmaciaDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public Lote Lote { get; set; } = new();

    public SelectList ListaProductos { get; set; } = null!;
    public SelectList ListaProveedores { get; set; } = null!;

    public async Task OnGetAsync()
    {
        await CargarDatos();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            await CargarDatos();
            return Page();
        }

        Lote.FechaIngreso = DateTime.Now;
        Lote.Activo = true;

        _context.Lotes.Add(Lote);
        await _context.SaveChangesAsync();

        await RegistrarBitacora("CREACION", "Lotes", $"ProductoID: {Lote.ProductoID}, Lote: {Lote.NumeroLote}, Cantidad: {Lote.Cantidad}");

        TempData["Success"] = $"Lote {Lote.NumeroLote} creado exitosamente.";
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

    private async Task RegistrarBitacora(string accion, string tabla, string valorNuevo)
    {
        var empleadoID = int.Parse(User.FindFirst("EmpleadoID")?.Value ?? "1");

        var bitacora = new BitacoraAuditoria
        {
            EmpleadoID = empleadoID,
            AccionEjecutada = accion,
            TablaAfectada = tabla,
            ValorNuevo = valorNuevo
        };
        _context.BitacoraAuditoria.Add(bitacora);
        await _context.SaveChangesAsync();
    }
}