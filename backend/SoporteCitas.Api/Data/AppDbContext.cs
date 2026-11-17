using Microsoft.EntityFrameworkCore;
using SoporteCitas.Api.Models;

namespace SoporteCitas.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Cita> Citas => Set<Cita>();
    public DbSet<RecuperacionPassword> RecuperacionesPassword => Set<RecuperacionPassword>();
    public DbSet<ConfiguracionHorario> ConfiguracionesHorario => Set<ConfiguracionHorario>();
    public DbSet<BloqueoHorario> BloqueosHorario => Set<BloqueoHorario>();

    protected override void OnModelCreating(ModelBuilder mb)
    {
        mb.Entity<Usuario>(e =>
        {
            e.HasIndex(u => u.Correo).IsUnique();
            e.Property(u => u.Nombre).HasMaxLength(100).IsRequired();
            e.Property(u => u.Correo).HasMaxLength(150).IsRequired();
            e.Property(u => u.Telefono).HasMaxLength(30);
        });

        mb.Entity<Cita>(e =>
        {
            e.Property(c => c.TipoServicio).HasMaxLength(100).IsRequired();
            e.Property(c => c.Descripcion).HasMaxLength(1000);
            e.Property(c => c.MotivoRespuesta).HasMaxLength(500);
            e.Property(c => c.Direccion).HasMaxLength(300);
            e.Property(c => c.ResumenTrabajo).HasMaxLength(1000);
            e.Property(c => c.NotasInternas).HasMaxLength(1000);

            // Las citas que ya existían quedan con modalidad "Local"
            e.Property(c => c.Modalidad).HasDefaultValue(ModalidadAtencion.Local);

            // Una sola cita activa (Pendiente=1 o Aceptada=2) por fecha/hora
            e.HasIndex(c => c.FechaHora)
             .IsUnique()
             .HasFilter("[Estado] IN (1, 2)");

            e.HasOne(c => c.Usuario)
             .WithMany(u => u.Citas)
             .HasForeignKey(c => c.UsuarioId);
        });

        mb.Entity<RecuperacionPassword>(e =>
        {
            e.Property(r => r.TokenHash).HasMaxLength(64).IsRequired();
            e.HasIndex(r => r.TokenHash).IsUnique();

            e.HasOne(r => r.Usuario)
             .WithMany()
             .HasForeignKey(r => r.UsuarioId);
        });

        mb.Entity<ConfiguracionHorario>(e =>
        {
            e.Property(c => c.DiasLaborables).HasMaxLength(20).IsRequired();
        });

        mb.Entity<BloqueoHorario>(e =>
        {
            e.Property(b => b.FechaInicio).HasColumnType("date");
            e.Property(b => b.FechaFin).HasColumnType("date");
            e.Property(b => b.Motivo).HasMaxLength(200);
        });
    }
}