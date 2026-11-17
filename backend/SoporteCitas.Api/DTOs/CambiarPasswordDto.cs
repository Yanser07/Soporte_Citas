using System.ComponentModel.DataAnnotations;

namespace SoporteCitas.Api.DTOs;

public class CambiarPasswordDto
{
    [Required, StringLength(128)]
    public string PasswordActual { get; set; } = string.Empty;

    [Required]
    [StringLength(64, MinimumLength = 8, ErrorMessage = "La contraseña debe tener entre 8 y 64 caracteres.")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).+$",
        ErrorMessage = "La contraseña debe incluir al menos una mayúscula, una minúscula y un número.")]
    public string NuevaPassword { get; set; } = string.Empty;
}