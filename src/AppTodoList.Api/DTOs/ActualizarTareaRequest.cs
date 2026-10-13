using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace AppTodoList.Api.DTOs;

public sealed class ActualizarTareaRequest
{
    [Required(ErrorMessage = "El título es obligatorio.")]
    [StringLength(
        200,
        MinimumLength = 1,
        ErrorMessage = "El título debe tener entre 1 y 200 caracteres.")]
    public string Title { get; init; } = string.Empty;

    [StringLength(2000, ErrorMessage = "La descripción no puede superar 2000 caracteres.")]
    public string? Description { get; init; }

    [JsonRequired]
    [EnumDataType(typeof(PrioridadTareaDto), ErrorMessage = "La prioridad indicada no es válida.")]
    public PrioridadTareaDto Priority { get; init; }

    public DateTimeOffset? DueDate { get; init; }
}
