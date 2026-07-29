using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using VitaNovaFarmacia.Data;
using VitaNovaFarmacia.Models;

namespace VitaNovaFarmacia.Pages.Clientes;

[Authorize]
public class EliminarModel : PageModel
{
    private readonly FarmaciaDbContext _context;

    public EliminarModel(FarmaciaDbContext context)
    {
        _context = context;
    }

    public Cliente Cliente { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int id)
    {
        Cliente = await _context.Clientes
            .FirstOrDefaultAsync(c => c.ID == id);

        if (Cliente == null)
        {
            return NotFound();
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        var cliente = await _context.Clientes
            .FirstOrDefaultAsync(c => c.ID == id);

        if (cliente == null)
        {
            return NotFound();
        }

        await RegistrarBitacora("ELIMINACION", "Clientes", $"Cliente: {cliente.Nombre} {cliente.Apellido}, Documento: {cliente.Documento}");

        _context.Clientes.Remove(cliente);
        await _context.SaveChangesAsync();

        TempData["Success"] = $"Cliente {cliente.Nombre} eliminado exitosamente.";
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