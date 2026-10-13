namespace AppTodoList.Api.DTOs;

public sealed record PaginaRespuesta<T>(
    IReadOnlyCollection<T> Items,
    int Page,
    int PageSize,
    int TotalCount,
    int TotalPages);
