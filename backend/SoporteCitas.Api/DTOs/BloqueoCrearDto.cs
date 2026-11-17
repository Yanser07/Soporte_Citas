using System.ComponentModel.DataAnnotations;

namespace SoporteCitas.Api.DTOs;

public class BloqueoCrearDto
{
    [Required]
    public DateTime FechaInicio { get; set; }

    // Si no se envía, el bloqueo es de un solo día
    public DateTime? FechaFin { get; set; }

    // Ambas null = día completo
    public int? HoraDesde { get; set; }
    public int? HoraHasta { get; set; }

    [StringLength(200)]
    public string? Motivo { get; set; }
}