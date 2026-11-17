using Microsoft.EntityFrameworkCore;
using SoporteCitas.Api.Data;
using SoporteCitas.Api.DTOs;
using SoporteCitas.Api.Models;

namespace SoporteCitas.Api.Services;

public interface IHorarioService
{
    Task<ConfiguracionHorario> ObtenerConfiguracionAsync();
    Task<DisponibilidadDto> ObtenerDisponibilidadAsync(DateOnly fecha);

    // Devuelve el mensaje de error, o null si la fecha es válida
    // (no revisa si otra cita ya ocupa ese horario; eso lo hace el controlador)
    Task<string?> ValidarFechaAsync(DateTime fecha);
}

public class HorarioService : IHorarioService
{
    private readonly AppDbContext _db;
    private readonly IRelojNegocio _reloj;

    public HorarioService(AppDbContext db, IRelojNegocio reloj)
    {
        _db = db;
        _reloj = reloj;
    }

    public async Task<ConfiguracionHorario> ObtenerConfiguracionAsync() =>
        await _db.ConfiguracionesHorario.AsNoTracking().FirstOrDefaultAsync()
        ?? new ConfiguracionHorario();

    public async Task<DisponibilidadDto> ObtenerDisponibilidadAsync(DateOnly fecha)
    {
        var config = await ObtenerConfiguracionAsync();
        var resultado = new DisponibilidadDto();

        // ¿Es un día laborable?
        if (!config.ObtenerDias().Contains((int)fecha.DayOfWeek))
        {
            resultado.Laborable = false;
            resultado.Mensaje = "Ese día no hay atención.";
            return resultado;
        }

        var inicioDia = fecha.ToDateTime(TimeOnly.MinValue);
        var finDia = inicioDia.AddDays(1);

        // Bloqueos que cubren este día
        var bloqueos = await _db.BloqueosHorario
            .AsNoTracking()
            .Where(b => b.FechaInicio <= inicioDia && b.FechaFin >= inicioDia)
            .ToListAsync();

        // ¿Día completo bloqueado?
        var bloqueoDia = bloqueos.FirstOrDefault(b => b.HoraDesde is null);
        if (bloqueoDia is not null)
        {
            resultado.Laborable = false;
            resultado.Mensaje = string.IsNullOrWhiteSpace(bloqueoDia.Motivo)
                ? "Ese día no hay atención."
                : $"Ese día no hay atención: {bloqueoDia.Motivo}.";
            return resultado;
        }

        var horasOcupadas = await _db.Citas
            .AsNoTracking()
            .Where(c => c.FechaHora >= inicioDia && c.FechaHora < finDia
                     && (c.Estado == EstadoCita.Pendiente || c.Estado == EstadoCita.Aceptada))
            .Select(c => c.FechaHora.Hour)
            .ToListAsync();

        var ahora = _reloj.Ahora;

        for (var hora = config.HoraInicio; hora < config.HoraFin; hora++)
        {
            var fechaHora = inicioDia.AddHours(hora);

            string? motivo = null;
            if (fechaHora <= ahora) motivo = "Horario pasado";
            else if (horasOcupadas.Contains(hora)) motivo = "Ocupado";
            else if (bloqueos.Any(b => hora >= b.HoraDesde && hora < b.HoraHasta)) motivo = "No disponible";

            resultado.Horarios.Add(new HorarioSlotDto
            {
                Hora = $"{hora:00}:00",
                FechaHora = fechaHora,
                Disponible = motivo is null,
                MotivoNoDisponible = motivo
            });
        }

        return resultado;
    }

    public async Task<string?> ValidarFechaAsync(DateTime fecha)
    {
        if (fecha != new DateTime(fecha.Year, fecha.Month, fecha.Day, fecha.Hour, 0, 0))
            return "Las citas son en bloques de 1 hora exacta (por ejemplo 9:00, 10:00).";

        var config = await ObtenerConfiguracionAsync();

        if (!config.ObtenerDias().Contains((int)fecha.DayOfWeek))
            return "Ese día no hay atención.";

        if (fecha.Hour < config.HoraInicio || fecha.Hour >= config.HoraFin)
            return $"El horario de atención es de {config.HoraInicio}:00 a {config.HoraFin}:00.";

        if (fecha <= _reloj.Ahora)
            return "La fecha y hora deben ser futuras.";

        var dia = fecha.Date;
        var hora = fecha.Hour;

        var bloqueado = await _db.BloqueosHorario.AnyAsync(b =>
            b.FechaInicio <= dia && b.FechaFin >= dia &&
            (b.HoraDesde == null || (hora >= b.HoraDesde && hora < b.HoraHasta)));

        if (bloqueado)
            return "Ese horario no está disponible.";

        return null;
    }
}