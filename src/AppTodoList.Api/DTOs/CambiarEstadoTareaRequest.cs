using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace AppTodoList.Api.DTOs;

public sealed class CambiarEstadoTareaRequest
{
    [JsonRequired]
    [EnumDataType(typeof(EstadoTareaDto), ErrorMessage = "El estado indicado no es válido.")]
    public EstadoTareaDto Status { get; init; }
}
