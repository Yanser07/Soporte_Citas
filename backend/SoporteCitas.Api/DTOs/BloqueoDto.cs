using SoporteCitas.Api.Models;

namespace SoporteCitas.Api.DTOs;

public class BloqueoDto
{
    public int Id { get; set; }
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
    public int? HoraDesde { get; set; }
    public int? HoraHasta { get; set; }
    public string? Motivo { get; set; }

    public static BloqueoDto Desde(BloqueoHorario b) => new()
    {
        Id = b.Id,
        FechaInicio = b.FechaInicio,
        FechaFin = b.FechaFin,
        HoraDesde = b.HoraDesde,
        HoraHasta = b.HoraHasta,
        Motivo = b.Motivo
    };
}