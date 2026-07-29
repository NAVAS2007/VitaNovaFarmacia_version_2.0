using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using VitaNovaFarmacia.Data;
using VitaNovaFarmacia.Models;

namespace VitaNovaFarmacia.Pages.Empleados;

[Authorize(Roles = "Administrador")]
public class EliminarModel : PageModel
{
    private readonly FarmaciaDbContext _context;

    public EliminarModel(FarmaciaDbContext context)
    {
        _context = context;
    }

    public Empleado Empleado { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int id)
    {
        Empleado = await _context.Empleados
            .Include(e => e.Rol)
            .FirstOrDefaultAsync(e => e.ID == id);

        if (Empleado == null)
        {
            return NotFound();
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        var empleado = await _context.Empleados
            .FirstOrDefaultAsync(e => e.ID == id);

        if (empleado == null)
        {
            return NotFound();
        }

        await RegistrarBitacora("ELIMINACION", "Empleados", $"Empleado: {empleado.Nombre}, Email: {empleado.Email}");

        _context.Empleados.Remove(empleado);
        await _context.SaveChangesAsync();

        TempData["Success"] = $"Empleado {empleado.Nombre} eliminado exitosamente.";
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