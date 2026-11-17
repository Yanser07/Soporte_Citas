using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using SoporteCitas.Api.Data;
using SoporteCitas.Api.DTOs;
using SoporteCitas.Api.Models;
using SoporteCitas.Api.Services;

namespace SoporteCitas.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PasswordController : ControllerBase
{
    private static readonly TimeSpan Vigencia = TimeSpan.FromMinutes(30);
    private static readonly TimeSpan EsperaEntreSolicitudes = TimeSpan.FromMinutes(2);

    private readonly AppDbContext _db;
    private readonly IEmailService _email;
    private readonly IConfiguration _config;

    public PasswordController(AppDbContext db, IEmailService email, IConfiguration config)
    {
        _db = db;
        _email = email;
        _config = config;
    }

    // POST api/password/olvide
    // Siempre responde lo mismo, exista o no el correo (evita descubrir qué correos están registrados)
    [HttpPost("olvide")]
    [EnableRateLimiting("recuperacion")]
    public async Task<IActionResult> Olvide(OlvidePasswordDto dto)
    {
        var respuesta = Ok(new
        {
            mensaje = "Si el correo está registrado, te enviamos un enlace para restablecer tu contraseña."
        });

        var correo = dto.Correo.Trim().ToLowerInvariant();
        var usuario = await _db.Usuarios.FirstOrDefaultAsync(u => u.Correo == correo);
        if (usuario is null) return respuesta;

        // Si ya se pidió un enlace hace poco, no se genera otro (evita el envío masivo de correos)
        var limite = DateTime.UtcNow - EsperaEntreSolicitudes;
        if (await _db.RecuperacionesPassword.AnyAsync(r => r.UsuarioId == usuario.Id && r.FechaCreacion > limite))
            return respuesta;

        // Los enlaces anteriores todavía vigentes quedan invalidados
        var vigentes = await _db.RecuperacionesPassword
            .Where(r => r.UsuarioId == usuario.Id && r.UsadoEn == null && r.Expira > DateTime.UtcNow)
            .ToListAsync();
        foreach (var anterior in vigentes) anterior.UsadoEn = DateTime.UtcNow;

        var token = GenerarToken();
        _db.RecuperacionesPassword.Add(new RecuperacionPassword
        {
            UsuarioId = usuario.Id,
            TokenHash = HashToken(token),
            Expira = DateTime.UtcNow.Add(Vigencia)
        });
        await _db.SaveChangesAsync();

        var urlBase = (_config["Frontend:Url"] ?? "http://localhost:5173").TrimEnd('/');
        var enlace = $"{urlBase}/restablecer?token={Uri.EscapeDataString(token)}";

        var nombre = usuario.Nombre;
        var destino = usuario.Correo;

        // El correo se envía en segundo plano para que la respuesta tarde lo mismo
        // exista o no la cuenta (así no se puede deducir por el tiempo de respuesta)
        _ = Task.Run(() => _email.NotificarRecuperacionPasswordAsync(nombre, destino, enlace));

        return respuesta;
    }

    // POST api/password/restablecer
    [HttpPost("restablecer")]
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> Restablecer(RestablecerPasswordDto dto)
    {
        var hash = HashToken(dto.Token.Trim());

        var registro = await _db.RecuperacionesPassword
            .Include(r => r.Usuario)
            .FirstOrDefaultAsync(r => r.TokenHash == hash);

        if (registro is null || registro.UsadoEn is not null || registro.Expira < DateTime.UtcNow)
            return BadRequest(new { mensaje = "El enlace no es válido o ya expiró. Solicita uno nuevo." });

        var usuario = registro.Usuario;
        usuario.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NuevaPassword, workFactor: 12);
        usuario.IntentosFallidos = 0;   // cambiar la contraseña también desbloquea la cuenta
        usuario.BloqueadoHasta = null;

        // El enlace usado y cualquier otro que siguiera vigente quedan invalidados
        var pendientes = await _db.RecuperacionesPassword
            .Where(r => r.UsuarioId == usuario.Id && r.UsadoEn == null)
            .ToListAsync();
        foreach (var pendiente in pendientes) pendiente.UsadoEn = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        var nombre = usuario.Nombre;
        var destino = usuario.Correo;
        _ = Task.Run(() => _email.NotificarPasswordCambiadaAsync(nombre, destino));

        return Ok(new { mensaje = "Contraseña actualizada. Ya puedes iniciar sesión." });
    }

    // ---------- Internos ----------

    private static string GenerarToken() =>
        WebEncoders.Base64UrlEncode(RandomNumberGenerator.GetBytes(32));

    private static string HashToken(string token) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
}