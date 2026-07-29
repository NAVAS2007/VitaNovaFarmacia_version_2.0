using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using VitaNovaFarmacia.Data;
using VitaNovaFarmacia.Models;

namespace VitaNovaFarmacia.Pages.Clientes;

[Authorize]
public class EditarModel : PageModel
{
    private readonly FarmaciaDbContext _context;

    public EditarModel(FarmaciaDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public Cliente Cliente { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var cliente = await _context.Clientes
            .FirstOrDefaultAsync(c => c.ID == id);

        if (cliente == null)
        {
            return NotFound();
        }

        Cliente = cliente;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var clienteBD = await _context.Clientes
            .FirstOrDefaultAsync(c => c.ID == Cliente.ID);

        if (clienteBD == null)
        {
            return NotFound();
        }

        var valorAnterior = $"Cliente: {clienteBD.Nombre} {clienteBD.Apellido}, Documento: {clienteBD.Documento}";

        clienteBD.Documento = Cliente.Documento;
        clienteBD.Nombre = Cliente.Nombre;
        clienteBD.Apellido = Cliente.Apellido;
        clienteBD.Telefono = Cliente.Telefono;
        clienteBD.Correo = Cliente.Correo;
        clienteBD.Direccion = Cliente.Direccion;
        clienteBD.Activo = Cliente.Activo;

        await _context.SaveChangesAsync();

        await RegistrarBitacora("EDICION", "Clientes", valorAnterior, $"Cliente: {clienteBD.Nombre} {clienteBD.Apellido}, Documento: {clienteBD.Documento}");

        TempData["Success"] = $"Cliente {clienteBD.Nombre} actualizado exitosamente.";
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