using System.ComponentModel.DataAnnotations;

namespace SoporteCitas.Api.DTOs;

public class RestablecerPasswordDto
{
    [Required, StringLength(200)]
    public string Token { get; set; } = string.Empty;

    [Required]
    [StringLength(64, MinimumLength = 8, ErrorMessage = "La contraseña debe tener entre 8 y 64 caracteres.")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).+$",
        ErrorMessage = "La contraseña debe incluir al menos una mayúscula, una minúscula y un número.")]
    public string NuevaPassword { get; set; } = string.Empty;
}