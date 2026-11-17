using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SoporteCitas.Api.Data;
using SoporteCitas.Api.DTOs;
using SoporteCitas.Api.Models;
using SoporteCitas.Api.Services;

namespace SoporteCitas.Api.Controllers;

[ApiController]
[Route("api/admin/citas")]
[Authorize(Roles = "Admin")]
public class AdminCitasController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IEmailService _email;
    private readonly IRelojNegocio _reloj;

    public AdminCitasController(AppDbContext db, IEmailService email, IRelojNegocio reloj)
    {
        _db = db;
        _email = email;
        _reloj = reloj;
    }

    // GET api/admin/citas?estado=Pendiente&buscar=juan&desde=2026-11-01&hasta=2026-11-30&pagina=1&tamanoPagina=10
    [HttpGet]
    public async Task<ActionResult<ResultadoPaginadoDto<CitaDto>>> Listar([FromQuery] FiltroCitasDto filtro)
    {
        var tamano = Math.Clamp(filtro.TamanoPagina, 1, 50);

        var query = _db.Citas.AsNoTracking().AsQueryable();

        if (filtro.Estado.HasValue)
        {
            var estado = filtro.Estado.Value;
            query = query.Where(c => c.Estado == estado);
        }

        var termino = filtro.Buscar?.Trim();
        if (!string.IsNullOrEmpty(termino))
        {
            if (termino.Length > 100) termino = termino[..100];

            query = query.Where(c =>
                c.Usuario.Nombre.Contains(termino) ||
                c.Usuario.Correo.Contains(termino) ||
                c.Usuario.Telefono.Contains(termino) ||
                c.TipoServicio.Contains(termino) ||
                c.Descripcion.Contains(termino));
        }

        if (filtro.Desde.HasValue)
        {
            var desde = filtro.Desde.Value.ToDateTime(TimeOnly.MinValue);
            query = query.Where(c => c.FechaHora >= desde);
        }

        if (filtro.Hasta.HasValue)
        {
            var limite = filtro.Hasta.Value.AddDays(1).ToDateTime(TimeOnly.MinValue);
            query = query.Where(c => c.FechaHora < limite);
        }

        var total = await query.CountAsync();
        var totalPaginas = Math.Max(1, (int)Math.Ceiling(total / (double)tamano));
        var pagina = Math.Clamp(filtro.Pagina, 1, totalPaginas);

        // Pendientes y aceptadas: las más próximas primero. El resto: las más recientes primero.
        var ascendente = filtro.Estado is EstadoCita.Pendiente or EstadoCita.Aceptada;
        var ordenada = ascendente
            ? query.OrderBy(c => c.FechaHora).ThenBy(c => c.Id)
            : query.OrderByDescending(c => c.FechaHora).ThenByDescending(c => c.Id);

        var citas = await ordenada
            .Include(c => c.Usuario)
            .Skip((pagina - 1) * tamano)
            .Take(tamano)
            .ToListAsync();

        return Ok(new ResultadoPaginadoDto<CitaDto>
        {
            Items = citas.Select(c => CitaDto.Desde(c, vistaAdmin: true)).ToList(),
            Total = total,
            Pagina = pagina,
            TamanoPagina = tamano,
            TotalPaginas = totalPaginas
        });
    }

    // GET api/admin/citas/resumen
    [HttpGet("resumen")]
    public async Task<IActionResult> Resumen()
    {
        var conteos = await _db.Citas
            .GroupBy(c => c.Estado)
            .Select(g => new { Estado = g.Key, Total = g.Count() })
            .ToListAsync();

        int Total(EstadoCita estado) => conteos.FirstOrDefault(x => x.Estado == estado)?.Total ?? 0;

        return Ok(new
        {
            pendientes = Total(EstadoCita.Pendiente),
            aceptadas = Total(EstadoCita.Aceptada),
            completadas = Total(EstadoCita.Completada),
            rechazadas = Total(EstadoCita.Rechazada),
            canceladas = Total(EstadoCita.Cancelada),
            total = conteos.Sum(x => x.Total)
        });
    }

    // GET api/admin/citas/5
    [HttpGet("{id:int}")]
    public async Task<ActionResult<CitaDto>> Obtener(int id)
    {
        var cita = await _db.Citas.Include(c => c.Usuario).FirstOrDefaultAsync(c => c.Id == id);

        if (cita is null)
            return NotFound(new { mensaje = "Cita no encontrada." });

        return Ok(CitaDto.Desde(cita, vistaAdmin: true));
    }

    // POST api/admin/citas/5/responder
    [HttpPost("{id:int}/responder")]
    public async Task<ActionResult<CitaDto>> Responder(int id, ResponderCitaDto dto)
    {
        var cita = await _db.Citas.Include(c => c.Usuario).FirstOrDefaultAsync(c => c.Id == id);

        if (cita is null)
            return NotFound(new { mensaje = "Cita no encontrada." });

        if (cita.Estado != EstadoCita.Pendiente)
            return BadRequest(new { mensaje = "Esta cita ya fue respondida o cancelada." });

        if (!dto.Aceptar && string.IsNullOrWhiteSpace(dto.Motivo))
            return BadRequest(new { mensaje = "Debes indicar un motivo al rechazar la cita." });

        cita.Estado = dto.Aceptar ? EstadoCita.Aceptada : EstadoCita.Rechazada;
        cita.MotivoRespuesta = string.IsNullOrWhiteSpace(dto.Motivo) ? null : dto.Motivo.Trim();
        cita.FechaRespuesta = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        // Correo al usuario con la respuesta y el motivo
        await _email.NotificarRespuestaAlUsuarioAsync(cita);

        return Ok(CitaDto.Desde(cita, vistaAdmin: true));
    }

    // POST api/admin/citas/5/completar
    [HttpPost("{id:int}/completar")]
    public async Task<ActionResult<CitaDto>> Completar(int id, CompletarCitaDto dto)
    {
        var cita = await _db.Citas.Include(c => c.Usuario).FirstOrDefaultAsync(c => c.Id == id);

        if (cita is null)
            return NotFound(new { mensaje = "Cita no encontrada." });

        if (cita.Estado != EstadoCita.Aceptada)
            return BadRequest(new { mensaje = "Solo se pueden completar las citas que están aceptadas." });

        if (cita.FechaHora > _reloj.Ahora)
            return BadRequest(new { mensaje = "Solo puedes marcar la cita como completada cuando llegue su hora." });

        cita.Estado = EstadoCita.Completada;
        cita.ResumenTrabajo = Limpiar(dto.ResumenTrabajo);
        cita.NotasInternas = Limpiar(dto.NotasInternas);
        cita.FechaCompletada = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        // Correo al cliente con el resumen del trabajo
        await _email.NotificarCitaCompletadaAsync(cita);

        return Ok(CitaDto.Desde(cita, vistaAdmin: true));
    }

    private static string? Limpiar(string? texto) =>
        string.IsNullOrWhiteSpace(texto) ? null : texto.Trim();
}