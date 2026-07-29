using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using VitaNovaFarmacia.Data;
using VitaNovaFarmacia.Models;

namespace VitaNovaFarmacia.Pages.Proveedores;

[Authorize(Roles = "Administrador,Regente")]
public class EditarModel : PageModel
{
    private readonly FarmaciaDbContext _context;

    public EditarModel(FarmaciaDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public Proveedor Proveedor { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var proveedor = await _context.Proveedores
            .FirstOrDefaultAsync(p => p.ID == id);

        if (proveedor == null)
        {
            return NotFound();
        }

        Proveedor = proveedor;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var proveedorBD = await _context.Proveedores
            .FirstOrDefaultAsync(p => p.ID == Proveedor.ID);

        if (proveedorBD == null)
        {
            return NotFound();
        }

        var valorAnterior = $"Proveedor: {proveedorBD.Nombre}, NIT: {proveedorBD.NIT}";

        proveedorBD.Nombre = Proveedor.Nombre;
        proveedorBD.Telefono = Proveedor.Telefono;
        proveedorBD.Correo = Proveedor.Correo;
        proveedorBD.Direccion = Proveedor.Direccion;
        proveedorBD.NIT = Proveedor.NIT;
        proveedorBD.Activo = Proveedor.Activo;

        await _context.SaveChangesAsync();

        await RegistrarBitacora("EDICION", "Proveedores", valorAnterior, $"Proveedor: {proveedorBD.Nombre}, NIT: {proveedorBD.NIT}");

        TempData["Success"] = $"Proveedor {proveedorBD.Nombre} actualizado exitosamente.";
        return RedirectToPage("./Index");
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