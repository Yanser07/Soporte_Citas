using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using SoporteCitas.Api.Data;
using SoporteCitas.Api.DTOs;
using SoporteCitas.Api.Models;
using SoporteCitas.Api.Services;

namespace SoporteCitas.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PerfilController : ControllerBase
{
    private const int MaxIntentos = 5;
    private static readonly TimeSpan DuracionBloqueo = TimeSpan.FromMinutes(15);

    private readonly AppDbContext _db;
    private readonly IEmailService _email;
    private readonly ILogger<PerfilController> _logger;

    public PerfilController(AppDbContext db, IEmailService email, ILogger<PerfilController> logger)
    {
        _db = db;
        _email = email;
        _logger = logger;
    }

    private int UsuarioId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    // GET api/perfil
    [HttpGet]
    public async Task<ActionResult<PerfilDto>> Obtener()
    {
        var usuario = await _db.Usuarios.AsNoTracking().FirstOrDefaultAsync(u => u.Id == UsuarioId);

        if (usuario is null)
            return Unauthorized();

        return Ok(Mapear(usuario));
    }

    // PUT api/perfil
    [HttpPut]
    public async Task<ActionResult<PerfilDto>> Actualizar(ActualizarPerfilDto dto)
    {
        var usuario = await _db.Usuarios.FirstOrDefaultAsync(u => u.Id == UsuarioId);

        if (usuario is null)
            return Unauthorized();

        usuario.Nombre = dto.Nombre.Trim();
        usuario.Telefono = dto.Telefono?.Trim() ?? string.Empty;
        await _db.SaveChangesAsync();

        return Ok(Mapear(usuario));
    }

    // POST api/perfil/cambiar-password
    [HttpPost("cambiar-password")]
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> CambiarPassword(CambiarPasswordDto dto)
    {
        var usuario = await _db.Usuarios.FirstOrDefaultAsync(u => u.Id == UsuarioId);

        if (usuario is null)
            return Unauthorized();

        if (usuario.BloqueadoHasta is { } hasta && hasta > DateTime.UtcNow)
        {
            var minutos = (int)Math.Ceiling((hasta - DateTime.UtcNow).TotalMinutes);
            return StatusCode(StatusCodes.Status429TooManyRequests, new
            {
                mensaje = $"Demasiados intentos fallidos. Inténtalo de nuevo en {minutos} minuto(s)."
            });
        }

        if (!BCrypt.Net.BCrypt.Verify(dto.PasswordActual, usuario.PasswordHash))
        {
            usuario.IntentosFallidos++;

            if (usuario.IntentosFallidos >= MaxIntentos)
            {
                usuario.BloqueadoHasta = DateTime.UtcNow.Add(DuracionBloqueo);
                usuario.IntentosFallidos = 0;
                _logger.LogWarning("Cuenta {UsuarioId} bloqueada por intentos fallidos al cambiar la contraseña.", usuario.Id);
            }

            await _db.SaveChangesAsync();
            return BadRequest(new { mensaje = "La contraseña actual no es correcta." });
        }

        if (dto.NuevaPassword == dto.PasswordActual)
            return BadRequest(new { mensaje = "La nueva contraseña debe ser distinta de la actual." });

        usuario.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NuevaPassword, workFactor: 12);
        usuario.IntentosFallidos = 0;
        usuario.BloqueadoHasta = null;
        await _db.SaveChangesAsync();

        await _email.NotificarPasswordCambiadaAsync(usuario.Nombre, usuario.Correo);

        return Ok(new { mensaje = "Contraseña actualizada correctamente." });
    }

    private static PerfilDto Mapear(Usuario u) => new()
    {
        Id = u.Id,
        Nombre = u.Nombre,
        Correo = u.Correo,
        Telefono = u.Telefono,
        Rol = u.Rol.ToString(),
        FechaCreacion = u.FechaCreacion
    };
}