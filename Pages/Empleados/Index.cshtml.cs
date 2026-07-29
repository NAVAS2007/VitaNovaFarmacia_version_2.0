using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using VitaNovaFarmacia.Data;
using VitaNovaFarmacia.Models;

namespace VitaNovaFarmacia.Pages.Empleados;

[Authorize(Roles = "Administrador")]
public class IndexModel : PageModel
{
    private readonly FarmaciaDbContext _context;

    public IndexModel(FarmaciaDbContext context)
    {
        _context = context;
    }

    public List<Empleado> Empleados { get; set; } = new();

    public async Task OnGetAsync()
    {
        Empleados = await _context.Empleados
            .Include(e => e.Rol)
            .AsNoTracking()
            .OrderBy(e => e.Nombre)
            .ToListAsync();

        // Agregar esto para depurar
        Console.WriteLine($"Empleados encontrados: {Empleados.Count}");
    }
}