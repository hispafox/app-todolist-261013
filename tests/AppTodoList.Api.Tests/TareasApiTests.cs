using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using AppTodoList.Api.DTOs;

namespace AppTodoList.Api.Tests;

public sealed class TareasApiTests
{
    private static readonly JsonSerializerOptions OpcionesJson = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    [Fact]
    public async Task Crear_y_consultar_tarea_aplica_valores_por_defecto_y_utc()
    {
        using var factory = new TareasApiFactory();
        using var cliente = factory.CreateClient();
        using var respuesta = await cliente.PostAsJsonAsync(
            "/api/tasks",
            new
            {
                title = "  Preparar la demo  ",
                dueDate = "2026-10-18T12:30:00+02:00"
            });

        Assert.Equal(HttpStatusCode.Created, respuesta.StatusCode);
        Assert.NotNull(respuesta.Headers.Location);

        var tarea = await respuesta.Content.ReadFromJsonAsync<TareaRespuesta>(OpcionesJson);
        Assert.NotNull(tarea);
        Assert.Equal("Preparar la demo", tarea.Title);
        Assert.Equal(EstadoTareaDto.Pending, tarea.Status);
        Assert.Equal(PrioridadTareaDto.Medium, tarea.Priority);
        Assert.Null(tarea.Description);
        Assert.Equal(tarea.CreatedAt, tarea.UpdatedAt);
        Assert.Equal(TimeSpan.Zero, tarea.CreatedAt.Offset);
        Assert.Equal(
            new DateTimeOffset(2026, 10, 18, 10, 30, 0, TimeSpan.Zero),
            tarea.DueDate);

        using var detalle = await cliente.GetAsync($"/api/tasks/{tarea.Id}");
        Assert.Equal(HttpStatusCode.OK, detalle.StatusCode);
    }

    [Fact]
    public async Task Rechaza_entradas_invalidas_sin_crear_tareas()
    {
        using var factory = new TareasApiFactory();
        using var cliente = factory.CreateClient();

        using var tituloVacio = await cliente.PostAsJsonAsync(
            "/api/tasks",
            new { title = "   " });
        using var tituloLargo = await cliente.PostAsJsonAsync(
            "/api/tasks",
            new { title = new string('x', 201) });
        using var estadoDesconocido = await cliente.PostAsJsonAsync(
            "/api/tasks",
            new { title = "Valida", status = "Paused" });
        using var filtroDesconocido = await cliente.GetAsync("/api/tasks?status=Paused");
        using var listado = await cliente.GetAsync("/api/tasks");
        var pagina = await listado.Content.ReadFromJsonAsync<PaginaRespuesta<TareaRespuesta>>(
            OpcionesJson);

        Assert.Equal(HttpStatusCode.BadRequest, tituloVacio.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, tituloLargo.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, estadoDesconocido.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, filtroDesconocido.StatusCode);
        Assert.NotNull(pagina);
        Assert.Empty(pagina.Items);
    }

    [Fact]
    public async Task Busca_filtra_y_pagina_en_el_servidor()
    {
        using var factory = new TareasApiFactory();
        using var cliente = factory.CreateClient();
        var primera = await CrearTareaAsync(cliente, new
        {
            title = "Comprar café",
            description = "Para la oficina",
            status = "InProgress",
            priority = "High"
        });
        await CrearTareaAsync(cliente, new
        {
            title = "Preparar reunión",
            description = "Revisar el CAFÉ de ventas",
            status = "Pending",
            priority = "Low"
        });
        await CrearTareaAsync(cliente, new
        {
            title = "Llamar al proveedor",
            description = "Seguimiento",
            status = "Pending",
            priority = "Medium"
        });

        using var combinados = await cliente.GetAsync(
            "/api/tasks?search=caF%C3%A9&status=InProgress&priority=High");
        var resultadoCombinado =
            await combinados.Content.ReadFromJsonAsync<PaginaRespuesta<TareaRespuesta>>(
                OpcionesJson);
        Assert.NotNull(resultadoCombinado);
        Assert.Single(resultadoCombinado.Items);
        Assert.Equal(primera.Id, resultadoCombinado.Items.Single().Id);

        using var primeraPagina = await cliente.GetAsync(
            "/api/tasks?search=CAF%C3%89&sortBy=createdAt&sortDirection=asc&pageSize=1&page=1");
        using var segundaPagina = await cliente.GetAsync(
            "/api/tasks?search=CAF%C3%89&sortBy=createdAt&sortDirection=asc&pageSize=1&page=2");
        var resultadoPrimero =
            await primeraPagina.Content.ReadFromJsonAsync<PaginaRespuesta<TareaRespuesta>>(
                OpcionesJson);
        var resultadoSegundo =
            await segundaPagina.Content.ReadFromJsonAsync<PaginaRespuesta<TareaRespuesta>>(
                OpcionesJson);

        Assert.NotNull(resultadoPrimero);
        Assert.NotNull(resultadoSegundo);
        Assert.Equal(2, resultadoPrimero.TotalCount);
        Assert.Equal(2, resultadoPrimero.TotalPages);
        Assert.NotEqual(
            resultadoPrimero.Items.Single().Id,
            resultadoSegundo.Items.Single().Id);

        using var tamanoExcesivo = await cliente.GetAsync("/api/tasks?pageSize=500");
        var resultadoLimitado =
            await tamanoExcesivo.Content.ReadFromJsonAsync<PaginaRespuesta<TareaRespuesta>>(
                OpcionesJson);
        Assert.NotNull(resultadoLimitado);
        Assert.Equal(100, resultadoLimitado.PageSize);
    }

    [Fact]
    public async Task Ordena_por_vencimiento_de_forma_estable_con_fechas_nulas_y_empates()
    {
        using var factory = new TareasApiFactory();
        using var cliente = factory.CreateClient();
        var sinVencimiento = await CrearTareaAsync(cliente, new { title = "Sin fecha" });
        var primera = await CrearTareaAsync(cliente, new
        {
            title = "Primera con fecha",
            dueDate = "2026-10-14T10:00:00Z"
        });
        var empate = await CrearTareaAsync(cliente, new
        {
            title = "Empate con fecha",
            dueDate = "2026-10-14T10:00:00Z"
        });
        var posterior = await CrearTareaAsync(cliente, new
        {
            title = "Fecha posterior",
            dueDate = "2026-10-15T10:00:00Z"
        });

        using var ascendente = await cliente.GetAsync(
            "/api/tasks?sortBy=dueDate&sortDirection=asc&pageSize=10");
        using var descendente = await cliente.GetAsync(
            "/api/tasks?sortBy=dueDate&sortDirection=desc&pageSize=10");
        var paginaAscendente =
            await ascendente.Content.ReadFromJsonAsync<PaginaRespuesta<TareaRespuesta>>(
                OpcionesJson);
        var paginaDescendente =
            await descendente.Content.ReadFromJsonAsync<PaginaRespuesta<TareaRespuesta>>(
                OpcionesJson);

        Assert.NotNull(paginaAscendente);
        Assert.NotNull(paginaDescendente);
        Assert.Equal(
            [sinVencimiento.Id, primera.Id, empate.Id, posterior.Id],
            paginaAscendente.Items.Select(tarea => tarea.Id));
        Assert.Equal(
            [posterior.Id, primera.Id, empate.Id, sinVencimiento.Id],
            paginaDescendente.Items.Select(tarea => tarea.Id));
    }

    [Fact]
    public async Task Ordena_por_prioridad_y_estado_en_ambas_direcciones_con_orden_estable()
    {
        using var factory = new TareasApiFactory();
        using var cliente = factory.CreateClient();
        var pendienteBaja = await CrearTareaAsync(cliente, new
        {
            title = "Pendiente baja",
            status = "Pending",
            priority = "Low"
        });
        var enProgresoAlta = await CrearTareaAsync(cliente, new
        {
            title = "En progreso alta",
            status = "InProgress",
            priority = "High"
        });
        var pendienteAlta = await CrearTareaAsync(cliente, new
        {
            title = "Pendiente alta",
            status = "Pending",
            priority = "High"
        });
        var completadaMedia = await CrearTareaAsync(cliente, new
        {
            title = "Completada media",
            status = "Completed",
            priority = "Medium"
        });

        using var prioridadAscendente = await cliente.GetAsync(
            "/api/tasks?sortBy=priority&sortDirection=asc&pageSize=10");
        using var prioridadDescendente = await cliente.GetAsync(
            "/api/tasks?sortBy=priority&sortDirection=desc&pageSize=10");
        using var estadoAscendente = await cliente.GetAsync(
            "/api/tasks?sortBy=status&sortDirection=asc&pageSize=10");
        using var estadoDescendente = await cliente.GetAsync(
            "/api/tasks?sortBy=status&sortDirection=desc&pageSize=10");
        var paginaPrioridadAscendente =
            await prioridadAscendente.Content.ReadFromJsonAsync<PaginaRespuesta<TareaRespuesta>>(
                OpcionesJson);
        var paginaPrioridadDescendente =
            await prioridadDescendente.Content.ReadFromJsonAsync<PaginaRespuesta<TareaRespuesta>>(
                OpcionesJson);
        var paginaEstadoAscendente =
            await estadoAscendente.Content.ReadFromJsonAsync<PaginaRespuesta<TareaRespuesta>>(
                OpcionesJson);
        var paginaEstadoDescendente =
            await estadoDescendente.Content.ReadFromJsonAsync<PaginaRespuesta<TareaRespuesta>>(
                OpcionesJson);

        Assert.NotNull(paginaPrioridadAscendente);
        Assert.NotNull(paginaPrioridadDescendente);
        Assert.NotNull(paginaEstadoAscendente);
        Assert.NotNull(paginaEstadoDescendente);
        Assert.Equal(
            [pendienteBaja.Id, completadaMedia.Id, enProgresoAlta.Id, pendienteAlta.Id],
            paginaPrioridadAscendente.Items.Select(tarea => tarea.Id));
        Assert.Equal(
            [enProgresoAlta.Id, pendienteAlta.Id, completadaMedia.Id, pendienteBaja.Id],
            paginaPrioridadDescendente.Items.Select(tarea => tarea.Id));
        Assert.Equal(
            [pendienteBaja.Id, pendienteAlta.Id, enProgresoAlta.Id, completadaMedia.Id],
            paginaEstadoAscendente.Items.Select(tarea => tarea.Id));
        Assert.Equal(
            [completadaMedia.Id, enProgresoAlta.Id, pendienteBaja.Id, pendienteAlta.Id],
            paginaEstadoDescendente.Items.Select(tarea => tarea.Id));
    }

    [Fact]
    public async Task Actualiza_sin_alterar_creacion_y_el_cambio_de_estado_es_idempotente()
    {
        using var factory = new TareasApiFactory();
        using var cliente = factory.CreateClient();
        var creada = await CrearTareaAsync(cliente, new
        {
            title = "Preparar material",
            priority = "Low"
        });

        await Task.Delay(5);
        using var actualizacion = await cliente.PutAsJsonAsync(
            $"/api/tasks/{creada.Id}",
            new
            {
                title = "Preparar material de clase",
                description = "Versión final",
                priority = "High",
                dueDate = (string?)null
            });
        Assert.Equal(HttpStatusCode.OK, actualizacion.StatusCode);
        var actualizada =
            await actualizacion.Content.ReadFromJsonAsync<TareaRespuesta>(OpcionesJson);
        Assert.NotNull(actualizada);
        Assert.Equal(creada.CreatedAt, actualizada.CreatedAt);
        Assert.True(actualizada.UpdatedAt > creada.UpdatedAt);
        Assert.Equal(EstadoTareaDto.Pending, actualizada.Status);
        Assert.Equal(PrioridadTareaDto.High, actualizada.Priority);

        using var cambioEstado = await cliente.PatchAsJsonAsync(
            $"/api/tasks/{creada.Id}/status",
            new { status = "InProgress" });
        var enProgreso =
            await cambioEstado.Content.ReadFromJsonAsync<TareaRespuesta>(OpcionesJson);
        Assert.NotNull(enProgreso);
        Assert.Equal(EstadoTareaDto.InProgress, enProgreso.Status);

        using var cambioRepetido = await cliente.PatchAsJsonAsync(
            $"/api/tasks/{creada.Id}/status",
            new { status = "InProgress" });
        var repetida =
            await cambioRepetido.Content.ReadFromJsonAsync<TareaRespuesta>(OpcionesJson);
        Assert.NotNull(repetida);
        Assert.Equal(enProgreso.UpdatedAt, repetida.UpdatedAt);
    }

    [Fact]
    public async Task Elimina_solo_tareas_existentes_y_responde_not_found_al_repetir()
    {
        using var factory = new TareasApiFactory();
        using var cliente = factory.CreateClient();
        var tarea = await CrearTareaAsync(cliente, new { title = "Eliminar" });

        using var eliminacion = await cliente.DeleteAsync($"/api/tasks/{tarea.Id}");
        using var eliminacionRepetida = await cliente.DeleteAsync($"/api/tasks/{tarea.Id}");
        using var detalle = await cliente.GetAsync($"/api/tasks/{tarea.Id}");

        Assert.Equal(HttpStatusCode.NoContent, eliminacion.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, eliminacionRepetida.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, detalle.StatusCode);
    }

    [Fact]
    public async Task Health_check_confirma_la_disponibilidad_de_sqlite()
    {
        using var factory = new TareasApiFactory();
        using var cliente = factory.CreateClient();

        using var respuesta = await cliente.GetAsync("/health");
        var contenido = await respuesta.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        Assert.Equal("Healthy", contenido);
    }

    private static async Task<TareaRespuesta> CrearTareaAsync(HttpClient cliente, object solicitud)
    {
        using var respuesta = await cliente.PostAsJsonAsync("/api/tasks", solicitud);
        respuesta.EnsureSuccessStatusCode();

        var tarea = await respuesta.Content.ReadFromJsonAsync<TareaRespuesta>(OpcionesJson);
        return tarea ?? throw new InvalidOperationException("La API no devolvió la tarea creada.");
    }
}
