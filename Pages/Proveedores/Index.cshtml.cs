using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using VitaNovaFarmacia.Data;
using VitaNovaFarmacia.Models;

namespace VitaNovaFarmacia.Pages.Proveedores;

[Authorize(Roles = "Administrador,Regente")]
public class IndexModel : PageModel
{
    private readonly FarmaciaDbContext _context;

    public IndexModel(FarmaciaDbContext context)
    {
        _context = context;
    }

    public List<Proveedor> Proveedores { get; set; } = new();

    public async Task OnGetAsync()
    {
        Proveedores = await _context.Proveedores
            .AsNoTracking()
            .OrderBy(p => p.Nombre)
            .ToListAsync();
    }
}