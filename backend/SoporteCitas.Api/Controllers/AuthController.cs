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
public class AuthController : ControllerBase
{
    private const int MaxIntentos = 5;
    private static readonly TimeSpan DuracionBloqueo = TimeSpan.FromMinutes(15);

    // Se usa cuando el correo no existe, para que la respuesta tarde lo mismo que con un correo real
    private static readonly string HashFalso = BCrypt.Net.BCrypt.HashPassword("hash-falso-para-igualar-tiempos");

    private readonly AppDbContext _db;
    private readonly ITokenService _tokens;
    private readonly ILogger<AuthController> _logger;

    public AuthController(AppDbContext db, ITokenService tokens, ILogger<AuthController> logger)
    {
        _db = db;
        _tokens = tokens;
        _logger = logger;
    }

    [HttpPost("registro")]
    [EnableRateLimiting("auth")]
    public async Task<ActionResult<AuthResponseDto>> Registro(RegistroDto dto)
    {
        var correo = dto.Correo.Trim().ToLowerInvariant();

        if (await _db.Usuarios.AnyAsync(u => u.Correo == correo))
            return Conflict(new { mensaje = "Ya existe una cuenta con ese correo." });

        var usuario = new Usuario
        {
            Nombre = dto.Nombre.Trim(),
            Correo = correo,
            Telefono = dto.Telefono?.Trim() ?? string.Empty,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password, workFactor: 12),
            Rol = RolUsuario.Usuario // el registro público NUNCA crea admins
        };

        _db.Usuarios.Add(usuario);
        await _db.SaveChangesAsync();

        return Ok(CrearRespuesta(usuario));
    }

    [HttpPost("login")]
    [EnableRateLimiting("auth")]
    public async Task<ActionResult<AuthResponseDto>> Login(LoginDto dto)
    {
        var correo = dto.Correo.Trim().ToLowerInvariant();
        var usuario = await _db.Usuarios.FirstOrDefaultAsync(u => u.Correo == correo);

        if (usuario is null)
        {
            BCrypt.Net.BCrypt.Verify(dto.Password, HashFalso);
            return CredencialesInvalidas();
        }

        if (usuario.BloqueadoHasta is { } hasta && hasta > DateTime.UtcNow)
        {
            var minutos = (int)Math.Ceiling((hasta - DateTime.UtcNow).TotalMinutes);
            return StatusCode(StatusCodes.Status429TooManyRequests, new
            {
                mensaje = $"Demasiados intentos fallidos. Inténtalo de nuevo en {minutos} minuto(s)."
            });
        }

        if (!BCrypt.Net.BCrypt.Verify(dto.Password, usuario.PasswordHash))
        {
            usuario.IntentosFallidos++;

            if (usuario.IntentosFallidos >= MaxIntentos)
            {
                usuario.BloqueadoHasta = DateTime.UtcNow.Add(DuracionBloqueo);
                usuario.IntentosFallidos = 0;
                _logger.LogWarning("Cuenta {UsuarioId} bloqueada por intentos fallidos de login.", usuario.Id);
            }

            await _db.SaveChangesAsync();
            return CredencialesInvalidas();
        }

        usuario.IntentosFallidos = 0;
        usuario.BloqueadoHasta = null;
        await _db.SaveChangesAsync();

        return Ok(CrearRespuesta(usuario));
    }

    // Sirve para probar que el token funciona
    [Authorize]
    [HttpGet("me")]
    public IActionResult Me()
    {
        return Ok(new
        {
            id = User.FindFirstValue(ClaimTypes.NameIdentifier),
            nombre = User.FindFirstValue(ClaimTypes.Name),
            correo = User.FindFirstValue(ClaimTypes.Email),
            rol = User.FindFirstValue(ClaimTypes.Role)
        });
    }

    private ActionResult CredencialesInvalidas() =>
        Unauthorized(new { mensaje = "Correo o contraseña incorrectos." });

    private AuthResponseDto CrearRespuesta(Usuario usuario)
    {
        var (token, expira) = _tokens.CrearToken(usuario);
        return new AuthResponseDto
        {
            Token = token,
            Expira = expira,
            UsuarioId = usuario.Id,
            Nombre = usuario.Nombre,
            Correo = usuario.Correo,
            Rol = usuario.Rol.ToString()
        };
    }
}