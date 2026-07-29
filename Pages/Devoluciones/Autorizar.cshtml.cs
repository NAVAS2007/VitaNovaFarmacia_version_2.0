using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using VitaNovaFarmacia.Data;
using VitaNovaFarmacia.Models;

namespace VitaNovaFarmacia.Pages.Devoluciones;

[Authorize(Roles = "Administrador,Regente")]
public class AutorizarModel : PageModel
{
    private readonly FarmaciaDbContext _context;

    public AutorizarModel(FarmaciaDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public Devolucion Devolucion { get; set; } = new();

    [BindProperty]
    public string Credencial { get; set; } = string.Empty;

    public async Task<IActionResult> OnGetAsync(int id)
    {
        Devolucion = await _context.Devoluciones
            .Include(d => d.Venta)
                .ThenInclude(v => v.Cliente)
            .Include(d => d.Empleado)
            .Include(d => d.Detalles)
                .ThenInclude(dd => dd.DetalleVenta)
                    .ThenInclude(dv => dv.Producto)
            .FirstOrDefaultAsync(d => d.ID == id);

        if (Devolucion == null)
        {
            return NotFound();
        }

        if (Devolucion.Estado != "Pendiente")
        {
            TempData["Error"] = "Esta devolucion ya fue procesada.";
            return RedirectToPage("./Index");
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var devolucion = await _context.Devoluciones
            .Include(d => d.Detalles)
            .FirstOrDefaultAsync(d => d.ID == Devolucion.ID);

        if (devolucion == null)
        {
            return NotFound();
        }

        // Verificar credenciales del regente
        var empleadoID = int.Parse(User.FindFirst("EmpleadoID")?.Value ?? "1");
        var regente = await _context.Empleados
            .FirstOrDefaultAsync(e => e.ID == empleadoID && e.Activo);

        if (regente == null)
        {
            TempData["Error"] = "Usuario no valido.";
            return RedirectToPage();
        }

        bool credencialValida = BCrypt.Net.BCrypt.Verify(Credencial, regente.PasswordHash);

        if (!credencialValida)
        {
            TempData["Error"] = "Credencial incorrecta. Acceso denegado.";
            return RedirectToPage();
        }

        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            // Revertir inventario
            foreach (var detalle in devolucion.Detalles)
            {
                var detalleVenta = await _context.DetalleVentas
                    .Include(d => d.Lote)
                    .FirstOrDefaultAsync(d => d.ID == detalle.DetalleVentaID);

                if (detalleVenta?.Lote != null)
                {
                    // Devolver al lote original
                    detalleVenta.Lote.Cantidad += detalle.Cantidad;
                    detalleVenta.Lote.Activo = true;
                }
            }

            // Actualizar estado
            devolucion.Estado = "Autorizada";
            devolucion.RegenteID = empleadoID;

            // Registrar en bitacora
            var bitacora = new BitacoraAuditoria
            {
                EmpleadoID = empleadoID,
                AccionEjecutada = "DEVOLUCION_AUTORIZADA",
                TablaAfectada = "Devoluciones",
                ValorNuevo = $"Devolucion #{devolucion.ID} - Venta #{devolucion.VentaID} - Total: {devolucion.TotalDevuelto:C}"
            };
            _context.BitacoraAuditoria.Add(bitacora);

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            TempData["Success"] = $"Devolucion #{devolucion.ID} autorizada exitosamente.";
            return RedirectToPage("./Index");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            TempData["Error"] = $"Error al autorizar devolucion: {ex.Message}";
            return RedirectToPage();
        }
    }
}