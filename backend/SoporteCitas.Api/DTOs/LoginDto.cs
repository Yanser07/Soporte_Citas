using System.ComponentModel.DataAnnotations;

namespace SoporteCitas.Api.DTOs;

public class LoginDto
{
    [Required, EmailAddress, StringLength(150)]
    public string Correo { get; set; } = string.Empty;

    [Required, StringLength(128)]
    public string Password { get; set; } = string.Empty;
}