using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using VitaNovaFarmacia.Data;
using VitaNovaFarmacia.Models;

namespace VitaNovaFarmacia.Pages.Lotes;

[Authorize(Roles = "Administrador,Regente")]
public class IndexModel : PageModel
{
    private readonly FarmaciaDbContext _context;

    public IndexModel(FarmaciaDbContext context)
    {
        _context = context;
    }

    public List<Lote> Lotes { get; set; } = new();

    public async Task OnGetAsync()
    {
        Lotes = await _context.Lotes
            .Include(l => l.Producto)
            .Include(l => l.Proveedor)
            .AsNoTracking()
            .OrderBy(l => l.FechaVencimiento)
            .ToListAsync();
    }
}