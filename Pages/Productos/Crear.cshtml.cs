using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using VitaNovaFarmacia.Data;
using VitaNovaFarmacia.Models;

namespace VitaNovaFarmacia.Pages.Productos;

public class CrearModel : PageModel
{
    private readonly FarmaciaDbContext _context;

    public CrearModel(FarmaciaDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public Producto Producto { get; set; } = new();

    [BindProperty]
    public int CantidadInicial { get; set; }

    public SelectList ListaCategorias { get; set; } = null!;

    public async Task OnGetAsync()
    {
        await CargarCategorias();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            await CargarCategorias();
            return Page();
        }

        Producto.Activo = true;
        _context.Productos.Add(Producto);
        await _context.SaveChangesAsync();

        // Crear lote inicial
        if (CantidadInicial > 0)
        {
            var lote = new Lote
            {
                ProductoID = Producto.ID,
                NumeroLote = "LOTE-" + DateTime.Now.ToString("yyyyMMddHHmmss"),
                Cantidad = CantidadInicial,
                CostoAdquisicion = 0, // Se puede actualizar despues
                FechaVencimiento = DateTime.Now.AddYears(1)
            };
            _context.Lotes.Add(lote);
            await _context.SaveChangesAsync();

            // Registrar en bitacora
            var bitacora = new BitacoraAuditoria
            {
                EmpleadoID = ObtenerEmpleadoID(),
                AccionEjecutada = "CREACION",
                TablaAfectada = "Productos",
                ValorNuevo = $"Producto: {Producto.Nombre}, Lote: {lote.NumeroLote}, Cantidad: {CantidadInicial}"
            };
            _context.BitacoraAuditoria.Add(bitacora);
            await _context.SaveChangesAsync();
        }

        return RedirectToPage("./Index");
    }

    private async Task CargarCategorias()
    {
        var categorias = await _context.Categorias
            .Where(c => c.Activa)
            .OrderBy(c => c.Nombre)
            .ToListAsync();

        ListaCategorias = new SelectList(categorias, "IdCategoria", "Nombre");
    }

    private int ObtenerEmpleadoID()
    {
        var claim = User.FindFirst("EmpleadoID");
        return claim != null ? int.Parse(claim.Value) : 1;
    }
}