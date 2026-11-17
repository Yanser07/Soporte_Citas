using SoporteCitas.Api.Models;

namespace SoporteCitas.Api.DTOs;

public class CitaDto
{
    public int Id { get; set; }
    public DateTime FechaHora { get; set; }
    public string TipoServicio { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public string Modalidad { get; set; } = string.Empty;
    public string? Direccion { get; set; }
    public string Estado { get; set; } = string.Empty;
    public string? MotivoRespuesta { get; set; }
    public string? ResumenTrabajo { get; set; }
    public DateTime FechaCreacion { get; set; }
    public DateTime? FechaRespuesta { get; set; }
    public DateTime? FechaCompletada { get; set; }

    // Solo se llenan en la vista del admin
    public string? ClienteNombre { get; set; }
    public string? ClienteCorreo { get; set; }
    public string? ClienteTelefono { get; set; }
    public string? NotasInternas { get; set; }

    public static CitaDto Desde(Cita c, bool vistaAdmin = false) => new()
    {
        Id = c.Id,
        FechaHora = c.FechaHora,
        TipoServicio = c.TipoServicio,
        Descripcion = c.Descripcion,
        Modalidad = c.Modalidad.ToString(),
        Direccion = c.Direccion,
        Estado = c.Estado.ToString(),
        MotivoRespuesta = c.MotivoRespuesta,
        ResumenTrabajo = c.ResumenTrabajo,
        FechaCreacion = c.FechaCreacion,
        FechaRespuesta = c.FechaRespuesta,
        FechaCompletada = c.FechaCompletada,
        ClienteNombre = vistaAdmin ? c.Usuario.Nombre : null,
        ClienteCorreo = vistaAdmin ? c.Usuario.Correo : null,
        ClienteTelefono = vistaAdmin ? c.Usuario.Telefono : null,
        NotasInternas = vistaAdmin ? c.NotasInternas : null
    };
}