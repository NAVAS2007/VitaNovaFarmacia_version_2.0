using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using VitaNovaFarmacia.Data;
using VitaNovaFarmacia.Models;

namespace VitaNovaFarmacia.Pages.Ventas;

[Authorize(Roles = "Administrador,Cajero")]
public class CarritoModel : PageModel
{
    private readonly FarmaciaDbContext _context;

    public CarritoModel(FarmaciaDbContext context)
    {
        _context = context;
    }

    public List<CarritoItem> Items { get; set; } = new();
    public decimal Total => Items.Sum(i => i.Subtotal);
    public List<Cliente> Clientes { get; set; } = new();
    public List<MetodoPago> MetodosPago { get; set; } = new();
    public int? VentaID { get; set; } // Para mostrar botón de factura

    [BindProperty]
    public int? ClienteID { get; set; }

    [BindProperty]
    public int MetodoPagoID { get; set; }

    [BindProperty]
    public decimal? Recibido { get; set; }

    public async Task OnGetAsync()
    {
        // Obtener VentaID desde TempData si existe
        if (TempData["VentaID"] != null && int.TryParse(TempData["VentaID"].ToString(), out int ventaID))
        {
            VentaID = ventaID;
        }

        await CargarDatos();
        CargarCarrito();
    }

    public async Task<IActionResult> OnGetBuscarAsync(string termino)
    {
        if (string.IsNullOrWhiteSpace(termino) || termino.Length < 2)
        {
            return new JsonResult(new List<object>());
        }

        var productos = await _context.Productos
            .Include(p => p.Lotes)
            .Where(p => p.Activo &&
                p.Lotes.Any(l => l.Cantidad > 0 && l.Activo) &&
                (p.Nombre.Contains(termino) ||
                 p.ComponenteActivo.Contains(termino) ||
                 p.CodigoBarras.Contains(termino)))
            .Select(p => new
            {
                id = p.ID,
                nombre = p.Nombre,
                componenteActivo = p.ComponenteActivo,
                precio = p.Precio,
                stock = p.Lotes.Where(l => l.Activo).Sum(l => l.Cantidad)
            })
            .Take(10)
            .ToListAsync();

        return new JsonResult(productos);
    }

    public async Task<IActionResult> OnPostAgregarAsync([FromBody] AgregarRequest request)
    {
        var carritoJson = HttpContext.Session.GetString("Carrito");
        var items = string.IsNullOrEmpty(carritoJson) ?
            new List<CarritoItem>() :
            JsonSerializer.Deserialize<List<CarritoItem>>(carritoJson) ?? new List<CarritoItem>();

        var stockTotal = await _context.Lotes
            .Where(l => l.ProductoID == request.ProductoID && l.Activo)
            .SumAsync(l => l.Cantidad);

        var itemExistente = items.FirstOrDefault(i => i.ProductoID == request.ProductoID);
        var cantidadActual = itemExistente?.Cantidad ?? 0;

        if (cantidadActual + request.Cantidad > stockTotal)
        {
            return new JsonResult(new { success = false, message = "No hay suficiente stock disponible" });
        }

        if (itemExistente != null)
        {
            itemExistente.Cantidad += request.Cantidad;
        }
        else
        {
            items.Add(new CarritoItem
            {
                ProductoID = request.ProductoID,
                Nombre = request.Nombre,
                ComponenteActivo = request.ComponenteActivo,
                Cantidad = request.Cantidad,
                PrecioUnitario = request.Precio
            });
        }

        HttpContext.Session.SetString("Carrito", JsonSerializer.Serialize(items));
        return new JsonResult(new { success = true });
    }

    public async Task<IActionResult> OnPostActualizarCantidadAsync([FromBody] ActualizarCantidadRequest request)
    {
        var carritoJson = HttpContext.Session.GetString("Carrito");
        if (string.IsNullOrEmpty(carritoJson))
        {
            return new JsonResult(new { success = false, message = "Carrito vacio" });
        }

        var items = JsonSerializer.Deserialize<List<CarritoItem>>(carritoJson);
        var item = items.FirstOrDefault(i => i.ProductoID == request.ProductoID);

        if (item == null)
        {
            return new JsonResult(new { success = false, message = "Producto no encontrado" });
        }

        var nuevaCantidad = item.Cantidad + request.Cantidad;
        var stockTotal = await _context.Lotes
            .Where(l => l.ProductoID == request.ProductoID && l.Activo)
            .SumAsync(l => l.Cantidad);

        if (nuevaCantidad > stockTotal)
        {
            return new JsonResult(new { success = false, message = "No hay suficiente stock" });
        }

        if (nuevaCantidad <= 0)
        {
            items.Remove(item);
        }
        else
        {
            item.Cantidad = nuevaCantidad;
        }

        HttpContext.Session.SetString("Carrito", JsonSerializer.Serialize(items));
        return new JsonResult(new { success = true });
    }

    public IActionResult OnPostEliminarAsync([FromBody] EliminarRequest request)
    {
        var carritoJson = HttpContext.Session.GetString("Carrito");
        if (string.IsNullOrEmpty(carritoJson))
        {
            return new JsonResult(new { success = false });
        }

        var items = JsonSerializer.Deserialize<List<CarritoItem>>(carritoJson);
        items.RemoveAll(i => i.ProductoID == request.ProductoID);

        HttpContext.Session.SetString("Carrito", JsonSerializer.Serialize(items));
        return new JsonResult(new { success = true });
    }

    public async Task<IActionResult> OnPostProcesarVentaAsync()
    {
        if (!ClienteID.HasValue)
        {
            TempData["Error"] = "No se pudo generar la compra.";
            await CargarDatos();
            CargarCarrito();
            return Page();
        }

        if (MetodoPagoID == 0)
        {
            TempData["Error"] = "Seleccione un metodo de pago.";
            await CargarDatos();
            CargarCarrito();
            return Page();
        }

        var carritoJson = HttpContext.Session.GetString("Carrito");
        if (string.IsNullOrEmpty(carritoJson))
        {
            TempData["Error"] = "El carrito esta vacio.";
            return RedirectToPage();
        }

        var items = JsonSerializer.Deserialize<List<CarritoItem>>(carritoJson);
        if (!items.Any())
        {
            TempData["Error"] = "El carrito esta vacio.";
            return RedirectToPage();
        }

        var empleadoID = int.Parse(User.FindFirst("EmpleadoID")?.Value ?? "1");

        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var venta = new Venta
            {
                ClienteID = ClienteID.Value,
                EmpleadoID = empleadoID,
                MetodoPagoID = MetodoPagoID,
                Total = items.Sum(i => i.Subtotal),
                Recibido = Recibido,
                Cambio = Recibido.HasValue ? Recibido.Value - items.Sum(i => i.Subtotal) : null,
                Estado = "Completada"
            };

            _context.Ventas.Add(venta);
            await _context.SaveChangesAsync();

            var detalles = new List<DetalleVenta>();

            foreach (var item in items)
            {
                var lotes = await _context.Lotes
                    .Where(l => l.ProductoID == item.ProductoID && l.Cantidad > 0 && l.Activo)
                    .OrderBy(l => l.FechaVencimiento)
                    .ToListAsync();

                var cantidadPendiente = item.Cantidad;

                foreach (var lote in lotes)
                {
                    if (cantidadPendiente <= 0) break;

                    var cantidadADescontar = Math.Min(cantidadPendiente, lote.Cantidad);

                    var detalle = new DetalleVenta
                    {
                        VentaID = venta.ID,
                        ProductoID = item.ProductoID,
                        LoteID = lote.ID,
                        Cantidad = cantidadADescontar,
                        PrecioUnitario = item.PrecioUnitario
                    };

                    detalles.Add(detalle);

                    lote.Cantidad -= cantidadADescontar;
                    if (lote.Cantidad == 0)
                    {
                        lote.Activo = false;
                    }

                    cantidadPendiente -= cantidadADescontar;
                }

                if (cantidadPendiente > 0)
                {
                    await transaction.RollbackAsync();
                    TempData["Error"] = $"No hay suficiente stock para {item.Nombre}. Faltan {cantidadPendiente} unidades.";
                    await CargarDatos();
                    CargarCarrito();
                    return Page();
                }
            }

            _context.DetalleVentas.AddRange(detalles);
            await _context.SaveChangesAsync();

            var bitacora = new BitacoraAuditoria
            {
                EmpleadoID = empleadoID,
                AccionEjecutada = "VENTA",
                TablaAfectada = "Ventas",
                ValorNuevo = $"Venta #{venta.ID} - Total: {venta.Total:C} - Cliente: {ClienteID}"
            };
            _context.BitacoraAuditoria.Add(bitacora);
            await _context.SaveChangesAsync();

            await transaction.CommitAsync();

            HttpContext.Session.Remove("Carrito");

            TempData["Success"] = $"Venta #{venta.ID} procesada exitosamente. Total: {venta.Total:C}";
            TempData["VentaID"] = venta.ID; // Guardar ID para mostrar botón de factura
            return RedirectToPage();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            TempData["Error"] = $"Error al procesar la venta: {ex.Message}";
            await CargarDatos();
            CargarCarrito();
            return Page();
        }
    }

    private void CargarCarrito()
    {
        var carritoJson = HttpContext.Session.GetString("Carrito");
        if (!string.IsNullOrEmpty(carritoJson))
        {
            Items = JsonSerializer.Deserialize<List<CarritoItem>>(carritoJson) ?? new List<CarritoItem>();
        }
    }

    private async Task CargarDatos()
    {
        Clientes = await _context.Clientes
            .Where(c => c.Activo)
            .OrderBy(c => c.Nombre)
            .ToListAsync();

        MetodosPago = await _context.MetodosPago
            .OrderBy(m => m.Nombre)
            .ToListAsync();
    } 

}

public class AgregarRequest
{
    public int ProductoID { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? ComponenteActivo { get; set; }
    public decimal Precio { get; set; }
    public int Cantidad { get; set; } = 1;
}

public class ActualizarCantidadRequest
{
    public int ProductoID { get; set; }
    public int Cantidad { get; set; }
}

public class EliminarRequest
{
    public int ProductoID { get; set; }
}

