using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using VitaNovaFarmacia.Data;
using VitaNovaFarmacia.Models;

namespace VitaNovaFarmacia.Pages.Empleados;

[Authorize(Roles = "Administrador")]
public class CrearModel : PageModel
{
    private readonly FarmaciaDbContext _context;

    public CrearModel(FarmaciaDbContext context)
    {
        _context = context;
    }

    // NO USAMOS [BindProperty] EN Empleado COMPLETO
    public Empleado Empleado { get; set; } = new();

    [BindProperty]
    public string Nombre { get; set; } = string.Empty;

    [BindProperty]
    public string Email { get; set; } = string.Empty;

    [BindProperty]
    public string PasswordInput { get; set; } = string.Empty;

    [BindProperty]
    public int RolID { get; set; }

    public SelectList ListaRoles { get; set; } = null!;

    public async Task OnGetAsync()
    {
        await CargarRoles();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        // Verificar que el modelo sea valido
        if (!ModelState.IsValid)
        {
            await CargarRoles();
            return Page();
        }

        // Verificar que la contraseña no este vacia
        if (string.IsNullOrWhiteSpace(PasswordInput))
        {
            ModelState.AddModelError("PasswordInput", "La contrasena es requerida.");
            await CargarRoles();
            return Page();
        }

        // Validar que el email no exista ya
        var existe = await _context.Empleados
            .AnyAsync(e => e.Email == Email);

        if (existe)
        {
            ModelState.AddModelError("Email", "Este correo electronico ya esta registrado.");
            await CargarRoles();
            return Page();
        }

        // Crear el empleado manualmente
        var empleado = new Empleado
        {
            Nombre = Nombre,
            Email = Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(PasswordInput),
            RolID = RolID,
            Activo = true,
            FechaIngreso = DateTime.Now
        };

        // Guardar en la base de datos
        _context.Empleados.Add(empleado);
        await _context.SaveChangesAsync();

        TempData["Success"] = $"Empleado {empleado.Nombre} creado exitosamente.";
        return RedirectToPage("./Index");
    }

    private async Task CargarRoles()
    {
        var roles = await _context.Roles
            .OrderBy(r => r.NombreRole)
            .ToListAsync();

        ListaRoles = new SelectList(roles, "ID", "NombreRole");
    }
}