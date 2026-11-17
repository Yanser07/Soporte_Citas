using SoporteCitas.Api.Models;

namespace SoporteCitas.Api.DTOs;

public class FiltroCitasDto
{
    public EstadoCita? Estado { get; set; }

    // Busca en nombre, correo y teléfono del cliente, en el tipo de servicio y en la descripción
    public string? Buscar { get; set; }

    // Rango de fechas de la cita (ambas incluidas)
    public DateOnly? Desde { get; set; }
    public DateOnly? Hasta { get; set; }

    public int Pagina { get; set; } = 1;
    public int TamanoPagina { get; set; } = 10;
}