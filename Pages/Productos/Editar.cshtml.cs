using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using VitaNovaFarmacia.Data;
using VitaNovaFarmacia.Models;

namespace VitaNovaFarmacia.Pages.Productos;

[Authorize(Roles = "Administrador,Regente")]
public class EditarModel : PageModel
{
    private readonly FarmaciaDbContext _context;

    public EditarModel(FarmaciaDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public Producto Producto { get; set; } = new();

    public SelectList ListaCategorias { get; set; } = null!;

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var producto = await _context.Productos
            .FirstOrDefaultAsync(p => p.ID == id);

        if (producto == null)
        {
            return NotFound();
        }

        Producto = producto;
        await CargarCategorias();

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            await CargarCategorias();
            return Page();
        }

        var productoBD = await _context.Productos
            .FirstOrDefaultAsync(p => p.ID == Producto.ID);

        if (productoBD == null)
        {
            return NotFound();
        }

        // Guardar valores anteriores para bitacora
        var valorAnterior = $"Producto: {productoBD.Nombre}, Precio: {productoBD.Precio}, StockMinimo: {productoBD.StockMinimo}";

        productoBD.Nombre = Producto.Nombre;
        productoBD.CodigoBarras = Producto.CodigoBarras;
        productoBD.ComponenteActivo = Producto.ComponenteActivo;
        productoBD.Precio = Producto.Precio;
        productoBD.StockMinimo = Producto.StockMinimo;
        productoBD.IdCategoria = Producto.IdCategoria;
        productoBD.Descripcion = Producto.Descripcion;
        productoBD.RequiereReceta = Producto.RequiereReceta;
        productoBD.Activo = Producto.Activo;

        await _context.SaveChangesAsync();

        // Registrar en bitacora
        await RegistrarBitacora("EDICION", "Productos", valorAnterior, $"Producto: {productoBD.Nombre}, Precio: {productoBD.Precio}, StockMinimo: {productoBD.StockMinimo}");

        TempData["Success"] = $"Producto {productoBD.Nombre} actualizado exitosamente.";
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