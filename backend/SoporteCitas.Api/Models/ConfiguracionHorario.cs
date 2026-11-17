namespace SoporteCitas.Api.Models;

public class ConfiguracionHorario
{
    public int Id { get; set; }

    // Días de la semana en que se atiende, como números separados por coma
    // (0 = domingo, 1 = lunes ... 6 = sábado). Por defecto: lunes a sábado.
    public string DiasLaborables { get; set; } = "1,2,3,4,5,6";

    public int HoraInicio { get; set; } = 8;
    public int HoraFin { get; set; } = 18;

    public List<int> ObtenerDias() =>
        DiasLaborables
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(int.Parse)
            .ToList();
}