namespace SoporteCitas.Api.Services;

public class EmailSettings
{
    public string Host { get; set; } = "smtp.gmail.com";
    public int Port { get; set; } = 587;
    public string Usuario { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string NombreRemitente { get; set; } = "Soporte Técnico";

    // Opcional: si está vacío se usa Admin:Correo
    public string? AdminDestino { get; set; }
}