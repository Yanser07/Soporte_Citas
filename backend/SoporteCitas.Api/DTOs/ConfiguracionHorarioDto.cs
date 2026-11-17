namespace SoporteCitas.Api.DTOs;

public class ConfiguracionHorarioDto
{
    // 0 = domingo, 1 = lunes ... 6 = sábado
    public List<int> DiasLaborables { get; set; } = new();
    public int HoraInicio { get; set; }
    public int HoraFin { get; set; }
}