using System.ComponentModel.DataAnnotations;

namespace SoporteCitas.Api.DTOs;

public class RegistroDto
{
    [Required, StringLength(100)]
    public string Nombre { get; set; } = string.Empty;

    [Required, EmailAddress, StringLength(150)]
    public string Correo { get; set; } = string.Empty;

    [Phone, StringLength(30)]
    public string Telefono { get; set; } = string.Empty;

    [Required]
    [StringLength(64, MinimumLength = 8, ErrorMessage = "La contraseña debe tener entre 8 y 64 caracteres.")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).+$",
        ErrorMessage = "La contraseña debe incluir al menos una mayúscula, una minúscula y un número.")]
    public string Password { get; set; } = string.Empty;
}