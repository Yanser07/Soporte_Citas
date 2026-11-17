using System.ComponentModel.DataAnnotations;

namespace SoporteCitas.Api.DTOs;

public class OlvidePasswordDto
{
    [Required, EmailAddress, StringLength(150)]
    public string Correo { get; set; } = string.Empty;
}