using AppTodoList.Api.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace AppTodoList.Api.Data;

public sealed class TareasDbContext(DbContextOptions<TareasDbContext> options)
    : DbContext(options)
{
    private static readonly ValueConverter<DateTimeOffset, long> ConversorFechaUtc = new(
        fecha => fecha.UtcDateTime.Ticks,
        ticks => new DateTimeOffset(ticks, TimeSpan.Zero));

    public DbSet<Tarea> Tareas => Set<Tarea>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var tarea = modelBuilder.Entity<Tarea>();

        tarea.ToTable("Tareas");
        tarea.HasKey(item => item.Id);
        tarea.Property(item => item.Titulo)
            .HasMaxLength(200)
            .IsRequired();
        tarea.Property(item => item.Descripcion)
            .HasMaxLength(2000);
        tarea.Property(item => item.Estado)
            .HasConversion<int>()
            .IsRequired();
        tarea.Property(item => item.Prioridad)
            .HasConversion<int>()
            .IsRequired();
        tarea.Property(item => item.CreadaEn)
            .HasConversion(ConversorFechaUtc)
            .HasColumnType("INTEGER")
            .IsRequired();
        tarea.Property(item => item.ActualizadaEn)
            .HasConversion(ConversorFechaUtc)
            .HasColumnType("INTEGER")
            .IsRequired();
        tarea.Property(item => item.VenceEn)
            .HasConversion(ConversorFechaUtc)
            .HasColumnType("INTEGER");
        tarea.Property(item => item.TextoBusqueda)
            .HasMaxLength(2201)
            .IsRequired();

        tarea.HasIndex(item => new { item.Estado, item.CreadaEn });
        tarea.HasIndex(item => new { item.Prioridad, item.CreadaEn });
        tarea.HasIndex(item => item.VenceEn);
    }
}
