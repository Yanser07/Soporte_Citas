using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SoporteCitas.Api.Data;
using SoporteCitas.Api.DTOs;
using SoporteCitas.Api.Models;
using SoporteCitas.Api.Services;

namespace SoporteCitas.Api.Controllers;

[ApiController]
[Route("api/admin/horario")]
[Authorize(Roles = "Admin")]
public class HorarioAdminController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IRelojNegocio _reloj;

    public HorarioAdminController(AppDbContext db, IRelojNegocio reloj)
    {
        _db = db;
        _reloj = reloj;
    }

    // GET api/admin/horario
    [HttpGet]
    public async Task<ActionResult<ConfiguracionHorarioDto>> Obtener()
    {
        var config = await ObtenerOCrearAsync();
        return Ok(Mapear(config));
    }

    // PUT api/admin/horario
    [HttpPut]
    public async Task<ActionResult<ConfiguracionHorarioDto>> Guardar(ConfiguracionHorarioDto dto)
    {
        var dias = dto.DiasLaborables.Distinct().OrderBy(d => d).ToList();

        if (dias.Count == 0 || dias.Any(d => d < 0 || d > 6))
            return BadRequest(new { mensaje = "Selecciona al menos un día laborable." });

        if (dto.HoraInicio < 0 || dto.HoraInicio > 23 ||
            dto.HoraFin < 1 || dto.HoraFin > 24 ||
            dto.HoraFin <= dto.HoraInicio)
            return BadRequest(new { mensaje = "El horario no es válido: la hora de cierre debe ser posterior a la de apertura." });

        var config = await ObtenerOCrearAsync();
        config.DiasLaborables = string.Join(',', dias);
        config.HoraInicio = dto.HoraInicio;
        config.HoraFin = dto.HoraFin;
        await _db.SaveChangesAsync();

        return Ok(Mapear(config));
    }

    // GET api/admin/horario/bloqueos  (solo los vigentes o futuros)
    [HttpGet("bloqueos")]
    public async Task<ActionResult<List<BloqueoDto>>> ListarBloqueos()
    {
        var hoy = _reloj.Hoy;

        var lista = await _db.BloqueosHorario
            .Where(b => b.FechaFin >= hoy)
            .OrderBy(b => b.FechaInicio)
            .ThenBy(b => b.HoraDesde)
            .ToListAsync();

        return Ok(lista.Select(BloqueoDto.Desde).ToList());
    }

    // POST api/admin/horario/bloqueos
    [HttpPost("bloqueos")]
    public async Task<IActionResult> CrearBloqueo(BloqueoCrearDto dto)
    {
        var inicio = dto.FechaInicio.Date;
        var fin = (dto.FechaFin ?? dto.FechaInicio).Date;

        if (fin < inicio)
            return BadRequest(new { mensaje = "La fecha final no puede ser anterior a la inicial." });

        if (fin < _reloj.Hoy)
            return BadRequest(new { mensaje = "El bloqueo no puede estar completamente en el pasado." });

        if (dto.HoraDesde.HasValue != dto.HoraHasta.HasValue)
            return BadRequest(new { mensaje = "Indica la hora de inicio y la de fin, o deja ambas vacías para bloquear el día completo." });

        if (dto.HoraDesde.HasValue &&
            (dto.HoraDesde < 0 || dto.HoraHasta > 24 || dto.HoraDesde >= dto.HoraHasta))
            return BadRequest(new { mensaje = "El rango de horas no es válido." });

        var bloqueo = new BloqueoHorario
        {
            FechaInicio = inicio,
            FechaFin = fin,
            HoraDesde = dto.HoraDesde,
            HoraHasta = dto.HoraHasta,
            Motivo = string.IsNullOrWhiteSpace(dto.Motivo) ? null : dto.Motivo.Trim()
        };

        _db.BloqueosHorario.Add(bloqueo);
        await _db.SaveChangesAsync();

        // Citas activas que quedaron dentro del bloqueo (no se cancelan solas: las revisa el admin)
        var limite = fin.AddDays(1);
        var query = _db.Citas.Where(c =>
            (c.Estado == EstadoCita.Pendiente || c.Estado == EstadoCita.Aceptada) &&
            c.FechaHora >= inicio && c.FechaHora < limite);

        if (bloqueo.HoraDesde.HasValue && bloqueo.HoraHasta.HasValue)
        {
            var desde = bloqueo.HoraDesde.Value;
            var hasta = bloqueo.HoraHasta.Value;
            query = query.Where(c => c.FechaHora.Hour >= desde && c.FechaHora.Hour < hasta);
        }

        var afectadas = await query.CountAsync();

        return Ok(new { bloqueo = BloqueoDto.Desde(bloqueo), citasAfectadas = afectadas });
    }

    // DELETE api/admin/horario/bloqueos/5
    [HttpDelete("bloqueos/{id:int}")]
    public async Task<IActionResult> EliminarBloqueo(int id)
    {
        var bloqueo = await _db.BloqueosHorario.FirstOrDefaultAsync(b => b.Id == id);

        if (bloqueo is null)
            return NotFound(new { mensaje = "Bloqueo no encontrado." });

        _db.BloqueosHorario.Remove(bloqueo);
        await _db.SaveChangesAsync();

        return NoContent();
    }

    // ---------- Internos ----------

    private async Task<ConfiguracionHorario> ObtenerOCrearAsync()
    {
        var config = await _db.ConfiguracionesHorario.FirstOrDefaultAsync();
        if (config is not null) return config;

        config = new ConfiguracionHorario();
        _db.ConfiguracionesHorario.Add(config);
        await _db.SaveChangesAsync();
        return config;
    }

    private static ConfiguracionHorarioDto Mapear(ConfiguracionHorario c) => new()
    {
        DiasLaborables = c.ObtenerDias(),
        HoraInicio = c.HoraInicio,
        HoraFin = c.HoraFin
    };
}