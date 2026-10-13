using AppTodoList.Api.DTOs;
using AppTodoList.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace AppTodoList.Api.Controllers;

[ApiController]
[Route("api/tasks")]
public sealed class TareasController(ITareaServicio servicio) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<PaginaRespuesta<TareaRespuesta>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginaRespuesta<TareaRespuesta>>> Listar(
        [FromQuery] FiltroTareasRequest filtros,
        CancellationToken cancellationToken)
    {
        var resultado = await servicio.ListarAsync(filtros, cancellationToken);
        return Ok(resultado);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType<TareaRespuesta>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TareaRespuesta>> Obtener(
        int id,
        CancellationToken cancellationToken)
    {
        var tarea = await servicio.ObtenerAsync(id, cancellationToken);

        return tarea is null
            ? Problem(statusCode: StatusCodes.Status404NotFound, title: "Tarea no encontrada")
            : Ok(tarea);
    }

    [HttpPost]
    [ProducesResponseType<TareaRespuesta>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TareaRespuesta>> Crear(
        CrearTareaRequest solicitud,
        CancellationToken cancellationToken)
    {
        var tarea = await servicio.CrearAsync(solicitud, cancellationToken);
        return CreatedAtAction(nameof(Obtener), new { id = tarea.Id }, tarea);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType<TareaRespuesta>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TareaRespuesta>> Actualizar(
        int id,
        ActualizarTareaRequest solicitud,
        CancellationToken cancellationToken)
    {
        var tarea = await servicio.ActualizarAsync(id, solicitud, cancellationToken);

        return tarea is null
            ? Problem(statusCode: StatusCodes.Status404NotFound, title: "Tarea no encontrada")
            : Ok(tarea);
    }

    [HttpPatch("{id:int}/status")]
    [ProducesResponseType<TareaRespuesta>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TareaRespuesta>> CambiarEstado(
        int id,
        CambiarEstadoTareaRequest solicitud,
        CancellationToken cancellationToken)
    {
        var tarea = await servicio.CambiarEstadoAsync(
            id,
            solicitud.Status,
            cancellationToken);

        return tarea is null
            ? Problem(statusCode: StatusCodes.Status404NotFound, title: "Tarea no encontrada")
            : Ok(tarea);
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Eliminar(int id, CancellationToken cancellationToken)
    {
        var eliminada = await servicio.EliminarAsync(id, cancellationToken);

        return eliminada
            ? NoContent()
            : Problem(statusCode: StatusCodes.Status404NotFound, title: "Tarea no encontrada");
    }
}
