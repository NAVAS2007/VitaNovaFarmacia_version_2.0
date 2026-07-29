using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using VitaNovaFarmacia.Data;
using VitaNovaFarmacia.Models;

namespace VitaNovaFarmacia.Pages.Proveedores;

[Authorize(Roles = "Administrador,Regente")]
public class EliminarModel : PageModel
{
    private readonly FarmaciaDbContext _context;

    public EliminarModel(FarmaciaDbContext context)
    {
        _context = context;
    }

    public Proveedor Proveedor { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int id)
    {
        Proveedor = await _context.Proveedores
            .FirstOrDefaultAsync(p => p.ID == id);

        if (Proveedor == null)
        {
            return NotFound();
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        var proveedor = await _context.Proveedores
            .FirstOrDefaultAsync(p => p.ID == id);

        if (proveedor == null)
        {
            return NotFound();
        }

        await RegistrarBitacora("ELIMINACION", "Proveedores", $"Proveedor: {proveedor.Nombre}, NIT: {proveedor.NIT}");

        _context.Proveedores.Remove(proveedor);
        await _context.SaveChangesAsync();

        TempData["Success"] = $"Proveedor {proveedor.Nombre} eliminado exitosamente.";
        return RedirectToPage("./Index");
    }

    private async Task RegistrarBitacora(string accion, string tabla, string valorAnterior)
    {
        var empleadoID = int.Parse(User.FindFirst("EmpleadoID")?.Value ?? "1");

        var bitacora = new BitacoraAuditoria
        {
            EmpleadoID = empleadoID,
            AccionEjecutada = accion,
            TablaAfectada = tabla,
            ValorAnterior = valorAnterior
        };
        _context.BitacoraAuditoria.Add(bitacora);
        await _context.SaveChangesAsync();
    }
}