namespace MCP.MicrosoftPlanner.Graph;

/// <summary>Datos mínimos de la identidad autenticada.</summary>
public sealed record IdentidadAutenticada(string? Id, string? NombreParaMostrar, string? NombrePrincipalUsuario);

/// <summary>Datos mínimos de un plan Planner básico.</summary>
public sealed record PlanPlanner(string? Id, string? Titulo, string? ETag);

/// <summary>Resultado de una búsqueda exacta de planes por nombre.</summary>
public sealed record ResultadoBusquedaPlanesPlanner(string NombreConsultado, IReadOnlyList<PlanPlanner> Coincidencias);

/// <summary>Datos mínimos de un bucket Planner.</summary>
public sealed record BucketPlanner(string? Id, string? Nombre, string? PlanId, string? ETag);

/// <summary>Datos mínimos de una tarea Planner.</summary>
public sealed record TareaPlanner(
    string? Id,
    string? Titulo,
    string? PlanId,
    string? BucketId,
    int? PorcentajeCompletado,
    DateTimeOffset? FechaVencimiento,
    string? ETag,
    IReadOnlyList<string>? Categorias = null,
    IReadOnlyList<string>? Asignaciones = null,
    DateTimeOffset? FechaInicio = null);

/// <summary>Datos detallados de solo lectura de una tarea Planner.</summary>
public sealed record DetalleTareaPlanner(
    string? Descripcion,
    string? ETag,
    IReadOnlyList<ElementoChecklistPlanner>? Checklist = null);

/// <summary>Campos mutables permitidos de una tarea Planner.</summary>
public sealed record ActualizacionTareaPlanner(
    string? Titulo,
    string? BucketId,
    int? PorcentajeCompletado,
    DateTimeOffset? FechaVencimiento,
    IReadOnlyList<string>? Categorias = null,
    IReadOnlyList<string>? Asignaciones = null,
    DateTimeOffset? FechaInicio = null,
    IReadOnlyList<string>? CategoriasDesmarcadas = null);

/// <summary>Elemento de checklist enviado al detalle de una tarea Planner; un título nulo elimina el elemento indicado.</summary>
public sealed record ElementoChecklistPlanner(string Id, string? Titulo, bool Completado);

/// <summary>Etiqueta definida en el detalle de un plan Planner básico.</summary>
public sealed record EtiquetaPlanPlanner(string Clave, string Nombre);

/// <summary>Detalle de un plan Planner básico con sus etiquetas y el ETag para actualizarlo.</summary>
public sealed record DetallesPlanPlanner(IReadOnlyList<EtiquetaPlanPlanner> Etiquetas, string? ETag);

/// <summary>Vista previa de etiquetas faltantes para una creación idempotente.</summary>
public sealed record VistaPreviaEtiquetasPlanner(IReadOnlyList<string> Existentes, IReadOnlyList<string> Faltantes);

/// <summary>Vista previa de buckets faltantes para una creación idempotente.</summary>
public sealed record VistaPreviaBucketsPlanner(IReadOnlyList<string> Existentes, IReadOnlyList<string> Faltantes);

/// <summary>Resultado seguro de una herramienta MCP.</summary>
public sealed record ResultadoHerramienta<T>(T? Datos, ErrorHerramienta? Error)
{
    public static ResultadoHerramienta<T> Exito(T datos) => new(datos, null);
    public static ResultadoHerramienta<T> Fallo(ErrorHerramienta error) => new(default, error);
}

/// <summary>Resultado de la eliminación de una tarea.</summary>
public sealed record ResultadoEliminacionTarea(string? TareaId, bool Eliminada);

/// <summary>Error estable y sin datos sensibles para consumidores MCP.</summary>
public sealed record ErrorHerramienta(string Codigo, string Mensaje, int? EstadoHttp = null);
