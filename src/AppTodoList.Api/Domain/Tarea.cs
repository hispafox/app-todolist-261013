using System.Text;

namespace AppTodoList.Api.Domain;

public sealed class Tarea
{
    private Tarea()
    {
    }

    public int Id { get; private set; }

    public string Titulo { get; private set; } = string.Empty;

    public string? Descripcion { get; private set; }

    public EstadoTarea Estado { get; private set; }

    public PrioridadTarea Prioridad { get; private set; }

    public DateTimeOffset CreadaEn { get; private set; }

    public DateTimeOffset ActualizadaEn { get; private set; }

    public DateTimeOffset? VenceEn { get; private set; }

    public string TextoBusqueda { get; private set; } = string.Empty;

    public static Tarea Crear(
        string titulo,
        string? descripcion,
        EstadoTarea estado,
        PrioridadTarea prioridad,
        DateTimeOffset? venceEn,
        DateTimeOffset ahora)
    {
        var tarea = new Tarea();
        tarea.AsignarDatos(titulo, descripcion, estado, prioridad, venceEn);
        tarea.CreadaEn = ahora.ToUniversalTime();
        tarea.ActualizadaEn = tarea.CreadaEn;
        return tarea;
    }

    public void Actualizar(
        string titulo,
        string? descripcion,
        PrioridadTarea prioridad,
        DateTimeOffset? venceEn,
        DateTimeOffset ahora)
    {
        AsignarDatos(titulo, descripcion, Estado, prioridad, venceEn);
        ActualizadaEn = ahora.ToUniversalTime();
    }

    public bool CambiarEstado(EstadoTarea estado, DateTimeOffset ahora)
    {
        if (Estado == estado)
        {
            return false;
        }

        Estado = estado;
        ActualizadaEn = ahora.ToUniversalTime();
        return true;
    }

    public bool CambiarPrioridad(PrioridadTarea prioridad, DateTimeOffset ahora)
    {
        if (Prioridad == prioridad)
        {
            return false;
        }

        Prioridad = prioridad;
        ActualizadaEn = ahora.ToUniversalTime();
        return true;
    }

    public static string NormalizarBusqueda(string texto) =>
        texto.Normalize(NormalizationForm.FormC).ToUpperInvariant();

    private void AsignarDatos(
        string titulo,
        string? descripcion,
        EstadoTarea estado,
        PrioridadTarea prioridad,
        DateTimeOffset? venceEn)
    {
        Titulo = titulo.Trim().Normalize(NormalizationForm.FormC);
        Descripcion = string.IsNullOrWhiteSpace(descripcion)
            ? null
            : descripcion.Trim().Normalize(NormalizationForm.FormC);
        Estado = estado;
        Prioridad = prioridad;
        VenceEn = venceEn?.ToUniversalTime();
        TextoBusqueda = NormalizarBusqueda($"{Titulo} {Descripcion}");
    }
}
