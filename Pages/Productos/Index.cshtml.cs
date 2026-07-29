using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using VitaNovaFarmacia.Data;

namespace VitaNovaFarmacia.Pages.Productos
{
    public class IndexModel : PageModel
    {
        private readonly FarmaciaDbContext _context;

        public IndexModel(FarmaciaDbContext context)
        {
            _context = context;
        }

        public List<ProductoData> Productos { get; set; } = new();
        public int TotalProductosStock { get; set; }
        public int ProductosStockCritico { get; set; }
        public int LotesPorVencer { get; set; }

        public async Task OnGetAsync()
        {
            TotalProductosStock = await _context.Lotes
                .Where(l => l.Activo)
                .SumAsync(l => l.Cantidad);

            ProductosStockCritico = await _context.Productos
                .Include(p => p.Lotes)
                .CountAsync(p => p.Activo &&
                    p.Lotes.Where(l => l.Activo).Sum(l => l.Cantidad) <= p.StockMinimo);

            var fechaLimite = DateTime.Now.AddDays(90);
            LotesPorVencer = await _context.Lotes
                .CountAsync(l => l.FechaVencimiento <= fechaLimite && l.Cantidad > 0 && l.Activo);

            var productosBD = await _context.Productos
                .Include(p => p.Lotes)
                .Include(p => p.Categoria)
                .Where(p => p.Activo)
                .AsNoTracking()
                .OrderBy(p => p.Nombre)
                .ToListAsync();

            Productos = productosBD.Select(p =>
            {
                var stockTotal = p.Lotes.Where(l => l.Activo).Sum(l => l.Cantidad);
                var loteActivo = p.Lotes
                    .Where(l => l.Cantidad > 0 && l.Activo)
                    .OrderBy(l => l.FechaVencimiento)
                    .FirstOrDefault();

                return new ProductoData
                {
                    ID = p.ID,
                    CodigoBarras = p.CodigoBarras,
                    Nombre = p.Nombre,
                    ComponenteActivo = p.ComponenteActivo,
                    Precio = p.Precio,
                    Categoria = p.Categoria?.Nombre ?? "Sin categoria",
                    StockTotal = stockTotal,
                    InfoLote = loteActivo != null ? $"Lote: {loteActivo.NumeroLote} - Vence: {loteActivo.FechaVencimiento:MM/yyyy}" : "Sin existencia",
                    EsCritico = stockTotal <= p.StockMinimo
                };
            }).ToList();
        }
    }

    public class ProductoData
    {
        public int ID { get; set; }
        public string? CodigoBarras { get; set; }
        public string? Nombre { get; set; }
        public string? ComponenteActivo { get; set; }
        public decimal Precio { get; set; }
        public string Categoria { get; set; } = string.Empty;
        public int StockTotal { get; set; }
        public string InfoLote { get; set; } = string.Empty;
        public bool EsCritico { get; set; }
    }
}