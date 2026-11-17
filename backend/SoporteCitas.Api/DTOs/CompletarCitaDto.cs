using System.ComponentModel.DataAnnotations;

namespace SoporteCitas.Api.DTOs;

public class CompletarCitaDto
{
    // Lo verá el cliente (historial y correo)
    [StringLength(1000)]
    public string? ResumenTrabajo { get; set; }

    // Privadas: solo las ve el admin
    [StringLength(1000)]
    public string? NotasInternas { get; set; }
}