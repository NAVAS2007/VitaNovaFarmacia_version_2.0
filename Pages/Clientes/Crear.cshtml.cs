using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using VitaNovaFarmacia.Data;
using VitaNovaFarmacia.Models;

namespace VitaNovaFarmacia.Pages.Clientes;

[Authorize]
public class CrearModel : PageModel
{
    private readonly FarmaciaDbContext _context;

    public CrearModel(FarmaciaDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public Cliente Cliente { get; set; } = new();

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        Cliente.FechaRegistro = DateTime.Now;
        Cliente.Activo = true;

        _context.Clientes.Add(Cliente);
        await _context.SaveChangesAsync();

        await RegistrarBitacora("CREACION", "Clientes", $"Cliente: {Cliente.Nombre} {Cliente.Apellido}, Documento: {Cliente.Documento}");

        TempData["Success"] = $"Cliente {Cliente.Nombre} creado exitosamente.";
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