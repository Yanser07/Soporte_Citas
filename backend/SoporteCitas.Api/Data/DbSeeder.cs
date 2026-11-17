using Microsoft.EntityFrameworkCore;
using SoporteCitas.Api.Models;

namespace SoporteCitas.Api.Data;

public static class DbSeeder
{
    public static async Task SeedAdminAsync(AppDbContext db, IConfiguration config)
    {
        if (await db.Usuarios.AnyAsync(u => u.Rol == RolUsuario.Admin)) return;

        var admin = new Usuario
        {
            Nombre = config["Admin:Nombre"] ?? "Administrador",
            Correo = config["Admin:Correo"]!,
            Telefono = config["Admin:Telefono"] ?? "",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(config["Admin:Password"]!),
            Rol = RolUsuario.Admin
        };

        db.Usuarios.Add(admin);
        await db.SaveChangesAsync();
    }

    public static async Task SeedHorarioAsync(AppDbContext db, IConfiguration config)
    {
        if (await db.ConfiguracionesHorario.AnyAsync()) return;

        db.ConfiguracionesHorario.Add(new ConfiguracionHorario
        {
            DiasLaborables = "1,2,3,4,5,6", // lunes a sábado
            HoraInicio = config.GetValue("Citas:HoraInicio", 8),
            HoraFin = config.GetValue("Citas:HoraFin", 18)
        });

        await db.SaveChangesAsync();
    }
}