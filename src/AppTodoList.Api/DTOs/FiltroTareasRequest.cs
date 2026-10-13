using System.ComponentModel.DataAnnotations;

namespace AppTodoList.Api.DTOs;

public sealed class FiltroTareasRequest
{
    [Range(1, int.MaxValue, ErrorMessage = "La página debe ser un número mayor que cero.")]
    public int Page { get; init; } = 1;

    [Range(1, int.MaxValue, ErrorMessage = "El tamaño de página debe ser mayor que cero.")]
    public int PageSize { get; init; } = 20;

    [StringLength(200, ErrorMessage = "La búsqueda no puede superar 200 caracteres.")]
    public string? Search { get; init; }

    [EnumDataType(typeof(EstadoTareaDto), ErrorMessage = "El estado indicado no es válido.")]
    public EstadoTareaDto? Status { get; init; }

    [EnumDataType(typeof(PrioridadTareaDto), ErrorMessage = "La prioridad indicada no es válida.")]
    public PrioridadTareaDto? Priority { get; init; }

    [RegularExpression(
        "(?i)^(createdAt|dueDate|priority|status)$",
        ErrorMessage = "El campo de orden debe ser createdAt, dueDate, priority o status.")]
    public string SortBy { get; init; } = "createdAt";

    [RegularExpression(
        "(?i)^(asc|desc)$",
        ErrorMessage = "La dirección debe ser asc o desc.")]
    public string SortDirection { get; init; } = "desc";
}
