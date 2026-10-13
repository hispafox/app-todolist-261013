namespace AppTodoList.Api.DTOs;

public sealed record TareaRespuesta(
    int Id,
    string Title,
    string? Description,
    EstadoTareaDto Status,
    PrioridadTareaDto Priority,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    DateTimeOffset? DueDate);
