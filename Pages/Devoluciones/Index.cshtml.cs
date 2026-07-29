using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using VitaNovaFarmacia.Data;
using VitaNovaFarmacia.Models;

namespace VitaNovaFarmacia.Pages.Devoluciones;

[Authorize(Roles = "Administrador,Regente")]
public class IndexModel : PageModel
{
    private readonly FarmaciaDbContext _context;

    public IndexModel(FarmaciaDbContext context)
    {
        _context = context;
    }

    public List<Devolucion> Devoluciones { get; set; } = new();

    public async Task OnGetAsync()
    {
        Devoluciones = await _context.Devoluciones
            .Include(d => d.Venta)
                .ThenInclude(v => v.Cliente)
            .Include(d => d.Empleado)
            .OrderByDescending(d => d.Fecha)
            .ToListAsync();
    }
}