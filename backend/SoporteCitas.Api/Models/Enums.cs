namespace SoporteCitas.Api.Models;

public enum RolUsuario
{
    Usuario = 1,
    Admin = 2
}

public enum EstadoCita
{
    Pendiente = 1,
    Aceptada = 2,
    Rechazada = 3,
    Cancelada = 4,
    Completada = 5
}

public enum ModalidadAtencion
{
    Remoto = 1,
    Local = 2,
    Domicilio = 3
}