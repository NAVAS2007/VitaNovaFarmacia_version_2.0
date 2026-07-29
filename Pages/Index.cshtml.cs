using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using VitaNovaFarmacia.Data;

namespace VitaNovaFarmacia.Pages;

public class IndexModel : PageModel
{
    private readonly FarmaciaDbContext _context;

    public IndexModel(FarmaciaDbContext context)
    {
        _context = context;
    }

    public int TotalProductosStock { get; set; }
    public int ProductosCriticos { get; set; }
    public int LotesPorVencer { get; set; }
    public int TotalClientes { get; set; }

    public async Task OnGetAsync()
    {
        TotalProductosStock = await _context.Lotes.SumAsync(l => l.Cantidad);

        ProductosCriticos = await _context.Productos
            .Include(p => p.Lotes)
            .CountAsync(p => p.Lotes.Sum(l => l.Cantidad) <= p.StockMinimo);

        var fechaLimite = DateTime.Now.AddDays(90);
        LotesPorVencer = await _context.Lotes
            .CountAsync(l => l.FechaVencimiento <= fechaLimite && l.Cantidad > 0);

        TotalClientes = await _context.Clientes.CountAsync(c => c.Activo);
    }
}