namespace SoporteCitas.Api.Models;

public class Cita
{
    public int Id { get; set; }

    public int UsuarioId { get; set; }
    public Usuario Usuario { get; set; } = null!;

    public DateTime FechaHora { get; set; }
    public string TipoServicio { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;

    public ModalidadAtencion Modalidad { get; set; } = ModalidadAtencion.Local;
    public string? Direccion { get; set; } // solo cuando la modalidad es Domicilio

    public EstadoCita Estado { get; set; } = EstadoCita.Pendiente;
    public string? MotivoRespuesta { get; set; }

    // Al completar el trabajo
    public string? ResumenTrabajo { get; set; } // lo ve el cliente
    public string? NotasInternas { get; set; }  // solo el admin
    public DateTime? FechaCompletada { get; set; }

    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    public DateTime? FechaRespuesta { get; set; }
}