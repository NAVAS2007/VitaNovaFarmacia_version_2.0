using System.ComponentModel.DataAnnotations;

namespace VitaNovaFarmacia.Models;

public class Categoria
{
    [Key]
    public int IdCategoria { get; set; }

    [Required]
    [StringLength(100)]
    public string Nombre { get; set; } = string.Empty;

    [StringLength(300)]
    public string? Descripcion { get; set; }

    public bool Activa { get; set; } = true;

    public ICollection<Producto> Productos { get; set; } = new List<Producto>();
}