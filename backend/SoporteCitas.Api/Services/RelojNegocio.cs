namespace SoporteCitas.Api.Services;

public interface IRelojNegocio
{
    // Fecha y hora actuales en la zona horaria del negocio
    DateTime Ahora { get; }

    // Fecha de hoy (sin hora) en la zona horaria del negocio
    DateTime Hoy { get; }
}

public class RelojNegocio : IRelojNegocio
{
    private readonly TimeZoneInfo _zona;

    public RelojNegocio(IConfiguration config)
    {
        var id = config["Citas:ZonaHoraria"] ?? "America/Santo_Domingo";

        try
        {
            _zona = TimeZoneInfo.FindSystemTimeZoneById(id);
        }
        catch (Exception ex) when (ex is TimeZoneNotFoundException or InvalidTimeZoneException)
        {
            throw new InvalidOperationException(
                $"No se encontró la zona horaria '{id}'. Revisa Citas:ZonaHoraria en la configuración.", ex);
        }
    }

    public DateTime Ahora =>
        DateTime.SpecifyKind(TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, _zona), DateTimeKind.Unspecified);

    public DateTime Hoy => Ahora.Date;
}