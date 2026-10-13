using AppTodoList.Api.Data;
using AppTodoList.Api.Domain;
using AppTodoList.Api.DTOs;
using Microsoft.EntityFrameworkCore;

namespace AppTodoList.Api.Services;

public sealed class ServicioTareas(TareasDbContext contexto) : ITareaServicio
{
    private const int TamanoPaginaMaximo = 100;

    public async Task<PaginaRespuesta<TareaRespuesta>> ListarAsync(
        FiltroTareasRequest filtros,
        CancellationToken cancellationToken)
    {
        var consulta = contexto.Tareas.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(filtros.Search))
        {
            var busqueda = Tarea.NormalizarBusqueda(filtros.Search.Trim());
            consulta = consulta.Where(tarea => tarea.TextoBusqueda.Contains(busqueda));
        }

        if (filtros.Status is { } estado)
        {
            var estadoDominio = MapeadorTareas.AEstadoDominio(estado);
            consulta = consulta.Where(tarea => tarea.Estado == estadoDominio);
        }

        if (filtros.Priority is { } prioridad)
        {
            var prioridadDominio = MapeadorTareas.APrioridadDominio(prioridad);
            consulta = consulta.Where(tarea => tarea.Prioridad == prioridadDominio);
        }

        var total = await consulta.CountAsync(cancellationToken);
        var tamanoPagina = Math.Min(filtros.PageSize, TamanoPaginaMaximo);
        var paginasTotales = (int)Math.Ceiling((double)total / tamanoPagina);
        var desplazamiento = ((long)filtros.Page - 1) * tamanoPagina;

        IReadOnlyCollection<TareaRespuesta> tareas;
        if (desplazamiento >= total)
        {
            tareas = [];
        }
        else
        {
            var elementos = await Ordenar(consulta, filtros.SortBy, filtros.SortDirection)
                .Skip((int)desplazamiento)
                .Take(tamanoPagina)
                .ToListAsync(cancellationToken);
            tareas = elementos.Select(MapeadorTareas.ARespuesta).ToArray();
        }

        return new PaginaRespuesta<TareaRespuesta>(
            tareas,
            filtros.Page,
            tamanoPagina,
            total,
            paginasTotales);
    }

    public async Task<TareaRespuesta?> ObtenerAsync(
        int id,
        CancellationToken cancellationToken)
    {
        var tarea = await contexto.Tareas
            .AsNoTracking()
            .SingleOrDefaultAsync(item => item.Id == id, cancellationToken);

        return tarea is null ? null : MapeadorTareas.ARespuesta(tarea);
    }

    public async Task<TareaRespuesta> CrearAsync(
        CrearTareaRequest solicitud,
        CancellationToken cancellationToken)
    {
        var estado = solicitud.Status is { } estadoDto
            ? MapeadorTareas.AEstadoDominio(estadoDto)
            : EstadoTarea.Pendiente;
        var prioridad = solicitud.Priority is { } prioridadDto
            ? MapeadorTareas.APrioridadDominio(prioridadDto)
            : PrioridadTarea.Media;
        var tarea = Tarea.Crear(
            solicitud.Title,
            solicitud.Description,
            estado,
            prioridad,
            solicitud.DueDate,
            DateTimeOffset.UtcNow);

        contexto.Tareas.Add(tarea);
        await contexto.SaveChangesAsync(cancellationToken);

        return MapeadorTareas.ARespuesta(tarea);
    }

    public async Task<TareaRespuesta?> ActualizarAsync(
        int id,
        ActualizarTareaRequest solicitud,
        CancellationToken cancellationToken)
    {
        var tarea = await contexto.Tareas
            .SingleOrDefaultAsync(item => item.Id == id, cancellationToken);

        if (tarea is null)
        {
            return null;
        }

        tarea.Actualizar(
            solicitud.Title,
            solicitud.Description,
            MapeadorTareas.APrioridadDominio(solicitud.Priority),
            solicitud.DueDate,
            DateTimeOffset.UtcNow);
        await contexto.SaveChangesAsync(cancellationToken);

        return MapeadorTareas.ARespuesta(tarea);
    }

    public async Task<TareaRespuesta?> CambiarEstadoAsync(
        int id,
        EstadoTareaDto estado,
        CancellationToken cancellationToken)
    {
        var tarea = await contexto.Tareas
            .SingleOrDefaultAsync(item => item.Id == id, cancellationToken);

        if (tarea is null)
        {
            return null;
        }

        if (tarea.CambiarEstado(MapeadorTareas.AEstadoDominio(estado), DateTimeOffset.UtcNow))
        {
            await contexto.SaveChangesAsync(cancellationToken);
        }

        return MapeadorTareas.ARespuesta(tarea);
    }

    public async Task<bool> EliminarAsync(int id, CancellationToken cancellationToken)
    {
        var tarea = await contexto.Tareas
            .SingleOrDefaultAsync(item => item.Id == id, cancellationToken);

        if (tarea is null)
        {
            return false;
        }

        contexto.Tareas.Remove(tarea);
        await contexto.SaveChangesAsync(cancellationToken);
        return true;
    }

    private static IOrderedQueryable<Tarea> Ordenar(
        IQueryable<Tarea> consulta,
        string campo,
        string direccion)
    {
        var descendente = direccion.Equals("desc", StringComparison.OrdinalIgnoreCase);

        return campo.ToLowerInvariant() switch
        {
            "duedate" => descendente
                ? consulta.OrderByDescending(tarea => tarea.VenceEn).ThenBy(tarea => tarea.Id)
                : consulta.OrderBy(tarea => tarea.VenceEn).ThenBy(tarea => tarea.Id),
            "priority" => descendente
                ? consulta.OrderByDescending(tarea => tarea.Prioridad).ThenBy(tarea => tarea.Id)
                : consulta.OrderBy(tarea => tarea.Prioridad).ThenBy(tarea => tarea.Id),
            "status" => descendente
                ? consulta.OrderByDescending(tarea => tarea.Estado).ThenBy(tarea => tarea.Id)
                : consulta.OrderBy(tarea => tarea.Estado).ThenBy(tarea => tarea.Id),
            _ => descendente
                ? consulta.OrderByDescending(tarea => tarea.CreadaEn).ThenBy(tarea => tarea.Id)
                : consulta.OrderBy(tarea => tarea.CreadaEn).ThenBy(tarea => tarea.Id)
        };
    }
}
