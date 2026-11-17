namespace SoporteCitas.Api.Models;

public class RecuperacionPassword
{
    public int Id { get; set; }

    public int UsuarioId { get; set; }
    public Usuario Usuario { get; set; } = null!;

    // Se guarda solo el hash (SHA-256) del token; el token real únicamente viaja en el correo
    public string TokenHash { get; set; } = string.Empty;

    public DateTime Expira { get; set; }
    public DateTime? UsadoEn { get; set; }
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
}