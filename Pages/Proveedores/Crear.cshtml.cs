using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using VitaNovaFarmacia.Data;
using VitaNovaFarmacia.Models;

namespace VitaNovaFarmacia.Pages.Proveedores;

[Authorize(Roles = "Administrador,Regente")]
public class CrearModel : PageModel
{
    private readonly FarmaciaDbContext _context;

    public CrearModel(FarmaciaDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public Proveedor Proveedor { get; set; } = new();

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        Proveedor.Activo = true;
        Proveedor.FechaRegistro = DateTime.Now;

        _context.Proveedores.Add(Proveedor);
        await _context.SaveChangesAsync();

        await RegistrarBitacora("CREACION", "Proveedores", $"Proveedor: {Proveedor.Nombre}, NIT: {Proveedor.NIT}");

        TempData["Success"] = $"Proveedor {Proveedor.Nombre} creado exitosamente.";
        return RedirectToPage("./Index");
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