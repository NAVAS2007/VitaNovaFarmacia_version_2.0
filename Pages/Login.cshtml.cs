using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using VitaNovaFarmacia.Data;

namespace VitaNovaFarmacia.Pages;

public class LoginModel : PageModel
{
    private readonly FarmaciaDbContext _context;

    public LoginModel(FarmaciaDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public string Email { get; set; } = string.Empty;

    [BindProperty]
    public string Password { get; set; } = string.Empty;

    public string ErrorMessage { get; set; } = string.Empty;

    // ============================================================
    // AGREGAR ESTA PROPIEDAD
    // ============================================================
    public string SuccessMessage { get; set; } = string.Empty;

    public void OnGet()
    {
        ErrorMessage = string.Empty;
        SuccessMessage = string.Empty;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        // Validar que los campos no esten vacios
        if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "Por favor ingrese correo y contrasena.";
            return Page();
        }

        // Buscar el empleado por email
        var empleado = await _context.Empleados
            .Include(e => e.Rol)
            .FirstOrDefaultAsync(e => e.Email.ToLower() == Email.ToLower() && e.Activo);

        if (empleado == null)
        {
            ErrorMessage = "Usuario no encontrado. Verifique su correo.";
            return Page();
        }

        // ============================================================
        // BYPASS: Acepta cualquier contraseña si el usuario existe
        // ============================================================
        bool esValida = false;

        if (Password == "123456")
        {
            esValida = true;
        }
        else
        {
            try
            {
                esValida = BCrypt.Net.BCrypt.Verify(Password, empleado.PasswordHash);
            }
            catch
            {
                esValida = false;
            }
        }

        if (!esValida)
        {
            ErrorMessage = "Correo electronico o contrasena incorrectos.";
            return Page();
        }

        // Actualizar ultimo login
        empleado.UltimoLogin = DateTime.Now;
        await _context.SaveChangesAsync();

        // Crear los claims para la autenticacion
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, empleado.Nombre),
            new Claim(ClaimTypes.Email, empleado.Email),
            new Claim(ClaimTypes.Role, empleado.Rol?.NombreRole ?? "Usuario"),
            new Claim("EmpleadoID", empleado.ID.ToString())
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

        SuccessMessage = $"Bienvenido {empleado.Nombre}!";

        // Redirigir segun el rol
        if (empleado.Rol?.NombreRole == "Cajero")
        {
            return RedirectToPage("/Ventas/Carrito");
        }
        else if (empleado.Rol?.NombreRole == "Regente")
        {
            return RedirectToPage("/Devoluciones/Index");
        }
        else
        {
            return RedirectToPage("/Productos/Index");
        }
    }

    public async Task<IActionResult> OnGetLogoutAsync()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToPage("/Login");
    }
}