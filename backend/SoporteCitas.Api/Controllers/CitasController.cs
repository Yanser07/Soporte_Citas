using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SoporteCitas.Api.Data;
using SoporteCitas.Api.DTOs;
using SoporteCitas.Api.Models;
using SoporteCitas.Api.Services;

namespace SoporteCitas.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CitasController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IEmailService _email;
    private readonly IHorarioService _horario;

    public CitasController(AppDbContext db, IEmailService email, IHorarioService horario)
    {
        _db = db;
        _email = email;
        _horario = horario;
    }

    private int UsuarioId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    // GET api/citas/disponibilidad?fecha=2026-11-10
    [HttpGet("disponibilidad")]
    public async Task<ActionResult<DisponibilidadDto>> Disponibilidad([FromQuery] DateOnly fecha)
    {
        return Ok(await _horario.ObtenerDisponibilidadAsync(fecha));
    }

    // POST api/citas
    [HttpPost]
    [Authorize(Roles = "Usuario")]
    public async Task<ActionResult<CitaDto>> Crear(CrearCitaDto dto)
    {
        if (!Enum.IsDefined(dto.Modalidad))
            return BadRequest(new { mensaje = "Selecciona la modalidad de atención." });

        var direccion = dto.Direccion?.Trim();
        if (dto.Modalidad == ModalidadAtencion.Domicilio && string.IsNullOrWhiteSpace(direccion))
            return BadRequest(new { mensaje = "Indica la dirección donde se realizará el servicio a domicilio." });

        var fecha = DateTime.SpecifyKind(dto.FechaHora, DateTimeKind.Unspecified);

        var error = await _horario.ValidarFechaAsync(fecha);
        if (error is not null)
            return BadRequest(new { mensaje = error });

        var ocupada = await _db.Citas.AnyAsync(c =>
            c.FechaHora == fecha &&
            (c.Estado == EstadoCita.Pendiente || c.Estado == EstadoCita.Aceptada));

        if (ocupada)
            return Conflict(new { mensaje = "Ese horario ya está ocupado. Elige otro." });

        var cita = new Cita
        {
            UsuarioId = UsuarioId,
            FechaHora = fecha,
            TipoServicio = dto.TipoServicio.Trim(),
            Descripcion = dto.Descripcion.Trim(),
            Modalidad = dto.Modalidad,
            Direccion = dto.Modalidad == ModalidadAtencion.Domicilio ? direccion : null,
            Estado = EstadoCita.Pendiente
        };

        _db.Citas.Add(cita);

        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            // Dos personas reservaron a la vez: el índice único de SQL Server lo detiene
            return Conflict(new { mensaje = "Ese horario acaba de ser ocupado. Elige otro." });
        }

        // Correo al admin avisando de la nueva solicitud
        var cliente = await _db.Usuarios.FirstAsync(u => u.Id == UsuarioId);
        await _email.NotificarNuevaCitaAlAdminAsync(cita, cliente);

        return Ok(CitaDto.Desde(cita));
    }

    // GET api/citas/mis-citas
    [HttpGet("mis-citas")]
    [Authorize(Roles = "Usuario")]
    public async Task<ActionResult<List<CitaDto>>> MisCitas()
    {
        var citas = await _db.Citas
            .Where(c => c.UsuarioId == UsuarioId)
            .OrderByDescending(c => c.FechaHora)
            .ToListAsync();

        return Ok(citas.Select(c => CitaDto.Desde(c)).ToList());
    }

    // POST api/citas/5/cancelar
    [HttpPost("{id:int}/cancelar")]
    [Authorize(Roles = "Usuario")]
    public async Task<ActionResult<CitaDto>> Cancelar(int id)
    {
        var cita = await _db.Citas
            .Include(c => c.Usuario)
            .FirstOrDefaultAsync(c => c.Id == id && c.UsuarioId == UsuarioId);

        if (cita is null)
            return NotFound(new { mensaje = "Cita no encontrada." });

        if (cita.Estado != EstadoCita.Pendiente && cita.Estado != EstadoCita.Aceptada)
            return BadRequest(new { mensaje = "Esta cita ya no se puede cancelar." });

        cita.Estado = EstadoCita.Cancelada;
        await _db.SaveChangesAsync();

        await _email.NotificarCancelacionAlAdminAsync(cita, cita.Usuario);

        return Ok(CitaDto.Desde(cita));
    }
}