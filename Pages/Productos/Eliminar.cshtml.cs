using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using VitaNovaFarmacia.Data;
using VitaNovaFarmacia.Models;

namespace VitaNovaFarmacia.Pages.Productos;

[Authorize(Roles = "Administrador,Regente")]
public class EliminarModel : PageModel
{
    private readonly FarmaciaDbContext _context;

    public EliminarModel(FarmaciaDbContext context)
    {
        _context = context;
    }

    public Producto Producto { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int id)
    {
        Producto = await _context.Productos
            .Include(p => p.Categoria)
            .Include(p => p.Lotes)
            .FirstOrDefaultAsync(p => p.ID == id);

        if (Producto == null)
        {
            return NotFound();
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        var producto = await _context.Productos
            .Include(p => p.Lotes)
            .FirstOrDefaultAsync(p => p.ID == id);

        if (producto == null)
        {
            return NotFound();
        }

        // Eliminar lotes asociados
        if (producto.Lotes != null && producto.Lotes.Any())
        {
            _context.Lotes.RemoveRange(producto.Lotes);
        }

        // Registrar en bitacora
        await RegistrarBitacora("ELIMINACION", "Productos", $"Producto: {producto.Nombre}, ID: {producto.ID}");

        _context.Productos.Remove(producto);
        await _context.SaveChangesAsync();

        TempData["Success"] = $"Producto {producto.Nombre} eliminado exitosamente.";
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