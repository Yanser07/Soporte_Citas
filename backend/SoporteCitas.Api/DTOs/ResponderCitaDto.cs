using System.ComponentModel.DataAnnotations;

namespace SoporteCitas.Api.DTOs;

public class ResponderCitaDto
{
    // true = aceptar, false = rechazar
    public bool Aceptar { get; set; }

    // Obligatorio al rechazar, opcional al aceptar
    [StringLength(500)]
    public string? Motivo { get; set; }
}