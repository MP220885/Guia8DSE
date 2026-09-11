using System.ComponentModel.DataAnnotations;

namespace PersonasAPI.Models;

public class Persona
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El primer nombre es requerido.")]
    [StringLength(100)]
    public string PrimerNombre { get; set; } = string.Empty;

    [StringLength(100)]
    public string? SegundoNombre { get; set; }

    [Required(ErrorMessage = "El primer apellido es requerido.")]
    [StringLength(100)]
    public string PrimerApellido { get; set; } = string.Empty;

    [StringLength(100)]
    public string? SegundoApellido { get; set; }

    [Required]
    [RegularExpression(@"^\d{8}-\d$", ErrorMessage = "El DUI debe tener el formato 01234567-8.")]
    public string Dui { get; set; } = string.Empty;

    [Required(ErrorMessage = "La fecha de nacimiento es requerida.")]
    public DateTime? FechaNacimiento { get; set; }
}
