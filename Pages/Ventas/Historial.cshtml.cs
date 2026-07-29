using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using VitaNovaFarmacia.Data;
using VitaNovaFarmacia.Models;

namespace VitaNovaFarmacia.Pages.Ventas;

[Authorize]
public class HistorialModel : PageModel
{
    private readonly FarmaciaDbContext _context;

    public HistorialModel(FarmaciaDbContext context)
    {
        _context = context;
    }

    public List<Venta> Ventas { get; set; } = new();

    public async Task OnGetAsync()
    {
        Ventas = await _context.Ventas
            .Include(v => v.Cliente)
            .Include(v => v.Empleado)
            .Include(v => v.MetodoPago)
            .OrderByDescending(v => v.Fecha)
            .Take(100)
            .ToListAsync();
    }
}