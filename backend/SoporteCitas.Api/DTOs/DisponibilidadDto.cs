namespace SoporteCitas.Api.DTOs;

public class DisponibilidadDto
{
    public bool Laborable { get; set; } = true;
    public string? Mensaje { get; set; }
    public List<HorarioSlotDto> Horarios { get; set; } = new();
}

public class HorarioSlotDto
{
    public string Hora { get; set; } = string.Empty;
    public DateTime FechaHora { get; set; }
    public bool Disponible { get; set; }
    public string? MotivoNoDisponible { get; set; }
}