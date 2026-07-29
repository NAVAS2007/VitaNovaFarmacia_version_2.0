using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using VitaNovaFarmacia.Data;
using VitaNovaFarmacia.Models;

namespace VitaNovaFarmacia.Pages.Clientes;

[Authorize]
public class IndexModel : PageModel
{
    private readonly FarmaciaDbContext _context;

    public IndexModel(FarmaciaDbContext context)
    {
        _context = context;
    }

    public List<Cliente> Clientes { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public string? BuscarTermino { get; set; }

    public async Task OnGetAsync()
    {
        var query = _context.Clientes.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(BuscarTermino))
        {
            query = query.Where(c =>
                c.Nombre.Contains(BuscarTermino) ||
                c.Documento.Contains(BuscarTermino) ||
                c.Telefono.Contains(BuscarTermino) ||
                c.Correo.Contains(BuscarTermino));
        }

        Clientes = await query
            .OrderBy(c => c.Nombre)
            .ToListAsync();
    }
}