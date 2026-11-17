using System.ComponentModel.DataAnnotations;
using SoporteCitas.Api.Models;

namespace SoporteCitas.Api.DTOs;

public class CrearCitaDto
{
    [Required]
    public DateTime FechaHora { get; set; }

    [Required, StringLength(100)]
    public string TipoServicio { get; set; } = string.Empty;

    [Required, StringLength(1000)]
    public string Descripcion { get; set; } = string.Empty;

    [Required]
    public ModalidadAtencion Modalidad { get; set; }

    // Obligatoria solo si la modalidad es Domicilio
    [StringLength(300)]
    public string? Direccion { get; set; }
}