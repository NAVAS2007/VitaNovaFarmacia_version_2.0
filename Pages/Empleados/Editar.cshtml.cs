using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using VitaNovaFarmacia.Data;
using VitaNovaFarmacia.Models;

namespace VitaNovaFarmacia.Pages.Empleados;

[Authorize(Roles = "Administrador")]
public class EditarModel : PageModel
{
    private readonly FarmaciaDbContext _context;

    public EditarModel(FarmaciaDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public Empleado Empleado { get; set; } = new();

    public SelectList ListaRoles { get; set; } = null!;

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var empleado = await _context.Empleados
            .FirstOrDefaultAsync(e => e.ID == id);

        if (empleado == null)
        {
            return NotFound();
        }

        Empleado = empleado;
        await CargarRoles();

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            await CargarRoles();
            return Page();
        }

        var empleadoBD = await _context.Empleados
            .FirstOrDefaultAsync(e => e.ID == Empleado.ID);

        if (empleadoBD == null)
        {
            return NotFound();
        }

        var valorAnterior = $"Empleado: {empleadoBD.Nombre}, Email: {empleadoBD.Email}, Rol: {empleadoBD.RolID}";

        empleadoBD.Nombre = Empleado.Nombre;
        empleadoBD.Email = Empleado.Email;
        empleadoBD.RolID = Empleado.RolID;
        empleadoBD.Activo = Empleado.Activo;

        await _context.SaveChangesAsync();

        await RegistrarBitacora("EDICION", "Empleados", valorAnterior, $"Empleado: {empleadoBD.Nombre}, Email: {empleadoBD.Email}, Rol: {empleadoBD.RolID}");

        TempData["Success"] = $"Empleado {empleadoBD.Nombre} actualizado exitosamente.";
        return RedirectToPage("./Index");
    }

    private async Task CargarRoles()
    {
        var roles = await _context.Roles
            .OrderBy(r => r.NombreRole)
            .ToListAsync();

        ListaRoles = new SelectList(roles, "ID", "NombreRole");
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