namespace SoporteCitas.Api.Models;

public class Usuario
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Correo { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public RolUsuario Rol { get; set; } = RolUsuario.Usuario;
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    // Protección contra fuerza bruta
    public int IntentosFallidos { get; set; }
    public DateTime? BloqueadoHasta { get; set; }

    public List<Cita> Citas { get; set; } = new();
}