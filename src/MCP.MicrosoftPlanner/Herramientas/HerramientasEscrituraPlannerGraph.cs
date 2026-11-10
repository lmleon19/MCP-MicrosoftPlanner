using System.ComponentModel;
using MCP.MicrosoftPlanner.Escritura;
using MCP.MicrosoftPlanner.Graph;
using ModelContextProtocol.Server;

namespace MCP.MicrosoftPlanner.Herramientas;

/// <summary>Expone operaciones mutantes genéricas de Planner con confirmación explícita.</summary>
[McpServerToolType]
public sealed class HerramientasEscrituraPlannerGraph(ServicioPlannerGraph servicioPlanner)
{
    /// <summary>Calcula los buckets solicitados que aún no existen en el plan.</summary>
    [McpServerTool(Name = "previsualizar_buckets", ReadOnly = true)]
    [Description("Lista los buckets existentes y los nombres faltantes, sin crear recursos.")]
    public async Task<ResultadoHerramienta<VistaPreviaBucketsPlanner>> PrevisualizarBucketsAsync(
        [Description("Identificador del plan Planner básico.")] string planId,
        [Description("Nombres de buckets que se desean asegurar.")] IReadOnlyList<string> nombres,
        CancellationToken cancellationToken)
    {
        return await HerramientasPlannerGraph.EjecutarAsync(async () =>
        {
            var existentes = await servicioPlanner.ListarBucketsAsync(planId, cancellationToken).ConfigureAwait(false);
            var nombresExistentes = existentes
                .Select(bucket => bucket.Nombre)
                .Where(nombre => !string.IsNullOrWhiteSpace(nombre))
                .Select(nombre => nombre!)
                .ToArray();
            var faltantes = NombresBucketsPlanner.ObtenerFaltantes(nombresExistentes, nombres);
            return new VistaPreviaBucketsPlanner(nombresExistentes, faltantes);
        }, "previsualizar buckets").ConfigureAwait(false);
    }

    /// <summary>Calcula las etiquetas solicitadas que aún no existen en el plan.</summary>
    [McpServerTool(Name = "previsualizar_categorias", ReadOnly = true)]
    [Description("Lista las etiquetas existentes y los nombres faltantes, sin crear recursos.")]
    public async Task<ResultadoHerramienta<VistaPreviaEtiquetasPlanner>> PrevisualizarCategoriasAsync(
        [Description("Identificador del plan Planner básico.")] string planId,
        [Description("Nombres de etiquetas que se desean asegurar.")] IReadOnlyList<string> nombres,
        CancellationToken cancellationToken)
    {
        return await HerramientasPlannerGraph.EjecutarAsync(async () =>
        {
            var detalle = await servicioPlanner.ObtenerDetallesPlanAsync(planId, cancellationToken).ConfigureAwait(false);
            var existentes = detalle.Etiquetas.Select(etiqueta => etiqueta.Nombre).ToArray();
            var faltantes = NombresEtiquetasPlanner.ObtenerFaltantes(existentes, nombres);
            return new VistaPreviaEtiquetasPlanner(existentes, faltantes);
        }, "previsualizar categorías").ConfigureAwait(false);
    }

    /// <summary>Crea los buckets indicados que no existan todavía.</summary>
    [McpServerTool(Name = "asegurar_buckets", ReadOnly = false)]
    [Description("Crea solamente los buckets faltantes después de una confirmación explícita.")]
    public async Task<ResultadoHerramienta<IReadOnlyList<BucketPlanner>>> AsegurarBucketsAsync(
        [Description("Identificador del plan Planner básico.")] string planId,
        [Description("Nombres de buckets que se desean asegurar.")] IReadOnlyList<string> nombres,
        [Description("Debe ser true para ejecutar la creación externa.")] bool confirmar,
        CancellationToken cancellationToken)
    {
        return await HerramientasPlannerGraph.EjecutarAsync(async () =>
        {
            GuardiaEscrituraPlanner.ValidarCreacion(confirmar);
            var existentes = await servicioPlanner.ListarBucketsAsync(planId, cancellationToken).ConfigureAwait(false);
            var existentesPorNombre = existentes
                .Where(bucket => !string.IsNullOrWhiteSpace(bucket.Nombre))
                .GroupBy(bucket => bucket.Nombre!.Trim(), StringComparer.OrdinalIgnoreCase)
                .ToDictionary(grupo => grupo.Key, grupo => grupo.First(), StringComparer.OrdinalIgnoreCase);
            var resultado = new List<BucketPlanner>(existentes);
            foreach (var nombre in NombresBucketsPlanner.ObtenerFaltantes(existentesPorNombre.Keys, nombres))
            {
                if (!existentesPorNombre.ContainsKey(nombre))
                {
                    var creado = await servicioPlanner.CrearBucketAsync(planId, nombre, cancellationToken).ConfigureAwait(false);
                    existentesPorNombre[nombre] = creado;
                    resultado.Add(creado);
                }
            }

            return (IReadOnlyList<BucketPlanner>)resultado;
        }, "asegurar buckets").ConfigureAwait(false);
    }

    /// <summary>Crea las etiquetas indicadas que no existan todavía.</summary>
    [McpServerTool(Name = "asegurar_categorias", ReadOnly = false)]
    [Description("Crea solamente las etiquetas faltantes después de una confirmación explícita.")]
    public Task<ResultadoHerramienta<DetallesPlanPlanner>> AsegurarCategoriasAsync(
        [Description("Identificador del plan Planner básico.")] string planId,
        [Description("Nombres de etiquetas que se desean asegurar.")] IReadOnlyList<string> nombres,
        [Description("Debe ser true para ejecutar la actualización externa.")] bool confirmar,
        CancellationToken cancellationToken)
        => HerramientasPlannerGraph.EjecutarAsync(() =>
        {
            GuardiaEscrituraPlanner.ValidarCreacion(confirmar);
            return servicioPlanner.AsegurarEtiquetasPlanAsync(planId, nombres, cancellationToken);
        }, "asegurar categorías");

    /// <summary>Crea una tarea en un plan y bucket indicados.</summary>
    [McpServerTool(Name = "crear_tarea", ReadOnly = false)]
    [Description("Crea una tarea Planner después de una confirmación explícita.")]
    public Task<ResultadoHerramienta<TareaPlanner>> CrearTareaAsync(
        string planId,
        string bucketId,
        string titulo,
        bool confirmar,
        CancellationToken cancellationToken)
        => HerramientasPlannerGraph.EjecutarAsync(() =>
        {
            GuardiaEscrituraPlanner.ValidarCreacion(confirmar);
            return servicioPlanner.CrearTareaAsync(planId, bucketId, titulo, cancellationToken);
        }, "crear tarea");

    /// <summary>Elimina una tarea Planner mediante su ETag y confirmación explícita.</summary>
    [McpServerTool(Name = "eliminar_tarea", ReadOnly = false)]
    [Description("Elimina una tarea Planner de forma permanente, mediante ETag y confirmación explícita.")]
    public Task<ResultadoHerramienta<ResultadoEliminacionTarea>> EliminarTareaAsync(
        string tareaId,
        string eTag,
        bool confirmar,
        CancellationToken cancellationToken)
        => HerramientasPlannerGraph.EjecutarAsync(async () =>
        {
            GuardiaEscrituraPlanner.Validar(confirmar, eTag);
            await servicioPlanner.EliminarTareaAsync(tareaId, eTag, cancellationToken).ConfigureAwait(false);
            return new ResultadoEliminacionTarea(tareaId, true);
        }, "eliminar tarea");

    /// <summary>Actualiza campos básicos de una tarea con ETag.</summary>
    [McpServerTool(Name = "actualizar_tarea", ReadOnly = false)]
    [Description("Actualiza título, bucket, progreso, fechas, categorías o asignaciones mediante ETag y confirmación.")]
    public Task<ResultadoHerramienta<TareaPlanner>> ActualizarTareaAsync(
        string tareaId,
        string eTag,
        ActualizacionTareaPlanner actualizacion,
        bool confirmar,
        CancellationToken cancellationToken)
        => HerramientasPlannerGraph.EjecutarAsync(() =>
        {
            GuardiaEscrituraPlanner.Validar(confirmar, eTag);
            return servicioPlanner.ActualizarTareaAsync(tareaId, eTag, actualizacion, cancellationToken);
        }, "actualizar tarea");

    /// <summary>Actualiza descripción y checklist de una tarea con ETag.</summary>
    [McpServerTool(Name = "actualizar_detalle_tarea", ReadOnly = false)]
    [Description("Actualiza descripción, agrega o actualiza elementos de checklist y permite retirar un elemento con título nulo, mediante ETag y confirmación.")]
    public Task<ResultadoHerramienta<DetalleTareaPlanner>> ActualizarDetalleTareaAsync(
        string tareaId,
        string eTag,
        string? descripcion,
        IReadOnlyList<ElementoChecklistPlanner>? checklist,
        bool confirmar,
        CancellationToken cancellationToken)
        => HerramientasPlannerGraph.EjecutarAsync(() =>
        {
            GuardiaEscrituraPlanner.Validar(confirmar, eTag);
            return servicioPlanner.ActualizarDetalleTareaAsync(tareaId, eTag, descripcion, checklist, cancellationToken);
        }, "actualizar detalle de tarea");
}
