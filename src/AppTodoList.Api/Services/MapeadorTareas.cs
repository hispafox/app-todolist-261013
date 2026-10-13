using AppTodoList.Api.Domain;
using AppTodoList.Api.DTOs;

namespace AppTodoList.Api.Services;

internal static class MapeadorTareas
{
    public static TareaRespuesta ARespuesta(Tarea tarea) =>
        new(
            tarea.Id,
            tarea.Titulo,
            tarea.Descripcion,
            ARespuesta(tarea.Estado),
            ARespuesta(tarea.Prioridad),
            tarea.CreadaEn,
            tarea.ActualizadaEn,
            tarea.VenceEn);

    public static EstadoTarea AEstadoDominio(EstadoTareaDto estado) =>
        estado switch
        {
            EstadoTareaDto.Pending => EstadoTarea.Pendiente,
            EstadoTareaDto.InProgress => EstadoTarea.EnProgreso,
            EstadoTareaDto.Completed => EstadoTarea.Completada,
            _ => throw new ArgumentOutOfRangeException(nameof(estado))
        };

    public static PrioridadTarea APrioridadDominio(PrioridadTareaDto prioridad) =>
        prioridad switch
        {
            PrioridadTareaDto.Low => PrioridadTarea.Baja,
            PrioridadTareaDto.Medium => PrioridadTarea.Media,
            PrioridadTareaDto.High => PrioridadTarea.Alta,
            _ => throw new ArgumentOutOfRangeException(nameof(prioridad))
        };

    private static EstadoTareaDto ARespuesta(EstadoTarea estado) =>
        estado switch
        {
            EstadoTarea.Pendiente => EstadoTareaDto.Pending,
            EstadoTarea.EnProgreso => EstadoTareaDto.InProgress,
            EstadoTarea.Completada => EstadoTareaDto.Completed,
            _ => throw new ArgumentOutOfRangeException(nameof(estado))
        };

    private static PrioridadTareaDto ARespuesta(PrioridadTarea prioridad) =>
        prioridad switch
        {
            PrioridadTarea.Baja => PrioridadTareaDto.Low,
            PrioridadTarea.Media => PrioridadTareaDto.Medium,
            PrioridadTarea.Alta => PrioridadTareaDto.High,
            _ => throw new ArgumentOutOfRangeException(nameof(prioridad))
        };
}
