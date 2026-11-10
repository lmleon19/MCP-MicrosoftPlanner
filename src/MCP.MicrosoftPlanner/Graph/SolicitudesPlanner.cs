namespace MCP.MicrosoftPlanner.Graph;

/// <summary>Construye rutas v1.0 de Microsoft Graph para operaciones Planner de solo lectura.</summary>
public static class SolicitudesPlanner
{
    private const string BaseGraph = "https://graph.microsoft.com/v1.0";

    /// <summary>Construye la ruta para obtener un plan.</summary>
    public static Uri Plan(string planId) => Crear("planner/plans", planId);

    /// <summary>Construye la ruta para obtener los buckets de un plan.</summary>
    public static Uri BucketsPlan(string planId) => Crear("planner/plans", planId, "buckets");

    /// <summary>Construye la ruta para obtener las tareas de un plan.</summary>
    public static Uri TareasPlan(string planId) => Crear("planner/plans", planId, "tasks");

    /// <summary>Construye la ruta para obtener una tarea.</summary>
    public static Uri Tarea(string tareaId) => Crear("planner/tasks", tareaId);

    /// <summary>Construye la ruta para obtener el detalle de una tarea.</summary>
    public static Uri DetalleTarea(string tareaId) => Crear("planner/tasks", tareaId, "details");

    private static Uri Crear(string segmentoInicial, string identificador, string? segmentoFinal = null)
    {
        if (string.IsNullOrWhiteSpace(identificador))
        {
            throw new ArgumentException("El identificador es obligatorio.", nameof(identificador));
        }

        var ruta = $"{segmentoInicial}/{Uri.EscapeDataString(identificador.Trim())}";
        if (segmentoFinal is not null)
        {
            ruta += $"/{segmentoFinal}";
        }

        return new Uri($"{BaseGraph}/{ruta}", UriKind.Absolute);
    }
}
