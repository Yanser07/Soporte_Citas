using System.ComponentModel.DataAnnotations;

namespace SoporteCitas.Api.DTOs;

public class ActualizarPerfilDto
{
    [Required, StringLength(100)]
    public string Nombre { get; set; } = string.Empty;

    [Phone, StringLength(30)]
    public string Telefono { get; set; } = string.Empty;
}