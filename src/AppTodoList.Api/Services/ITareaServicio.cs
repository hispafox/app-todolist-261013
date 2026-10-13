using AppTodoList.Api.DTOs;

namespace AppTodoList.Api.Services;

public interface ITareaServicio
{
    Task<PaginaRespuesta<TareaRespuesta>> ListarAsync(
        FiltroTareasRequest filtros,
        CancellationToken cancellationToken);

    Task<TareaRespuesta?> ObtenerAsync(int id, CancellationToken cancellationToken);

    Task<TareaRespuesta> CrearAsync(
        CrearTareaRequest solicitud,
        CancellationToken cancellationToken);

    Task<TareaRespuesta?> ActualizarAsync(
        int id,
        ActualizarTareaRequest solicitud,
        CancellationToken cancellationToken);

    Task<TareaRespuesta?> CambiarEstadoAsync(
        int id,
        EstadoTareaDto estado,
        CancellationToken cancellationToken);

    Task<bool> EliminarAsync(int id, CancellationToken cancellationToken);
}
