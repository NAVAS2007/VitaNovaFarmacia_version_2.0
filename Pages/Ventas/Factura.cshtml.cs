using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using VitaNovaFarmacia.Data;
using VitaNovaFarmacia.Models;
using VitaNovaFarmacia.Services;

namespace VitaNovaFarmacia.Pages.Ventas;

public class FacturaModel : PageModel
{
    private readonly FarmaciaDbContext _context;
    private readonly PdfService _pdfService;
    private readonly EmailService _emailService;

    public Venta? Venta { get; set; }
    public string? MensajeExito { get; set; }
    public string? MensajeError { get; set; }

    public FacturaModel(FarmaciaDbContext context, PdfService pdfService, EmailService emailService)
    {
        _context = context;
        _pdfService = pdfService;
        _emailService = emailService;
    }

    public IActionResult OnGet(int? id)
    {
        if (!id.HasValue)
            return RedirectToPage("/Ventas/Historial");

        Venta = _context.Ventas
            .Include(v => v.Cliente)
            .Include(v => v.Empleado)
            .Include(v => v.MetodoPago)
            .Include(v => v.DetalleVentas)
            .ThenInclude(dv => dv.Producto)
            .FirstOrDefault(v => v.ID == id);

        if (Venta == null)
            return NotFound();

        return Page();
    }

    public IActionResult OnPostDescargarPdf(int id)
    {
        try
        {
            var venta = _context.Ventas
                .Include(v => v.Cliente)
                .Include(v => v.Empleado)
                .Include(v => v.MetodoPago)
                .Include(v => v.DetalleVentas)
                .ThenInclude(dv => dv.Producto)
                .FirstOrDefault(v => v.ID == id);

            if (venta == null)
                return NotFound();

            var pdfBytes = _pdfService.GenerarFacturaPdf(id);
            return File(pdfBytes, "application/pdf", $"Factura_{id}_{DateTime.Now:yyyyMMdd}.pdf");
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Error al generar PDF: {ex.Message}";
            return RedirectToPage(new { id });
        }
    }

    public async Task<IActionResult> OnPostEnviarEmailAsync(int id, string? correosAdicionales)
    {
        try
        {
            var venta = _context.Ventas
                .Include(v => v.Cliente)
                .Include(v => v.Empleado)
                .Include(v => v.MetodoPago)
                .Include(v => v.DetalleVentas)
                .ThenInclude(dv => dv.Producto)
                .FirstOrDefault(v => v.ID == id);

            if (venta == null)
                return NotFound();

            // Lista de correos a enviar
            var correosEnvio = new List<string>();

            // Agregar correo del cliente
            if (!string.IsNullOrEmpty(venta.Cliente?.Correo))
            {
                correosEnvio.Add(venta.Cliente.Correo);
            }

            // Agregar correos adicionales si los hay
            if (!string.IsNullOrEmpty(correosAdicionales))
            {
                var adicionales = correosAdicionales
                    .Split(',')
                    .Select(c => c.Trim())
                    .Where(c => !string.IsNullOrEmpty(c) && c.Contains("@"))
                    .Distinct()
                    .ToList();

                correosEnvio.AddRange(adicionales);
            }

            // Validar que haya al menos un correo
            if (correosEnvio.Count == 0)
            {
                TempData["Error"] = "No hay correos para enviar. El cliente no tiene correo registrado.";
                return RedirectToPage(new { id });
            }

            // Generar PDF una sola vez
            var pdfBytes = _pdfService.GenerarFacturaPdf(id);

            // Enviar a todos los correos
            int correosExitosos = 0;
            var correosConError = new List<string>();

            foreach (var correo in correosEnvio)
            {
                try
                {
                    var enviado = await _emailService.EnviarFacturaPorEmailAsync(
                        correo,
                        $"{venta.Cliente?.Nombre} {venta.Cliente?.Apellido}",
                        pdfBytes,
                        venta.ID
                    );

                    if (enviado)
                        correosExitosos++;
                    else
                        correosConError.Add(correo);
                }
                catch (Exception ex)
                {
                    correosConError.Add($"{correo} ({ex.Message})");
                }
            }

            // Mensaje de resultado
            if (correosExitosos > 0)
            {
                TempData["Exito"] = $"✅ Factura enviada a {correosExitosos} correo(s)";
            }

            if (correosConError.Count > 0)
            {
                TempData["Advertencia"] = $"⚠️ Hubo errores al enviar a: {string.Join(", ", correosConError)}";
            }

            return RedirectToPage(new { id });
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Error al enviar email: {ex.Message}";
            return RedirectToPage(new { id });
        }
    }
}
