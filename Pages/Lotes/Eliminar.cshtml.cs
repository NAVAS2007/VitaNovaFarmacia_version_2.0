using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using VitaNovaFarmacia.Data;
using VitaNovaFarmacia.Models;

namespace VitaNovaFarmacia.Pages.Lotes;

[Authorize(Roles = "Administrador,Regente")]
public class EliminarModel : PageModel
{
    private readonly FarmaciaDbContext _context;

    public EliminarModel(FarmaciaDbContext context)
    {
        _context = context;
    }

    public Lote Lote { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int id)
    {
        Lote = await _context.Lotes
            .Include(l => l.Producto)
            .FirstOrDefaultAsync(l => l.ID == id);

        if (Lote == null)
        {
            return NotFound();
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        var lote = await _context.Lotes
            .FirstOrDefaultAsync(l => l.ID == id);

        if (lote == null)
        {
            return NotFound();
        }

        await RegistrarBitacora("ELIMINACION", "Lotes", $"Lote: {lote.NumeroLote}, ProductoID: {lote.ProductoID}, Cantidad: {lote.Cantidad}");

        _context.Lotes.Remove(lote);
        await _context.SaveChangesAsync();

        TempData["Success"] = $"Lote {lote.NumeroLote} eliminado exitosamente.";
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