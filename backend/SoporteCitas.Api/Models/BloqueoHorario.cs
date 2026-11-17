namespace SoporteCitas.Api.Models;

public class BloqueoHorario
{
    public int Id { get; set; }

    // Rango de fechas (ambas incluidas)
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }

    // Si ambas son null, se bloquea el día completo.
    // Si tienen valor, se bloquean las horas desde HoraDesde hasta antes de HoraHasta, cada día del rango.
    public int? HoraDesde { get; set; }
    public int? HoraHasta { get; set; }

    // Los clientes verán este texto cuando intenten reservar un día bloqueado
    public string? Motivo { get; set; }

    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
}