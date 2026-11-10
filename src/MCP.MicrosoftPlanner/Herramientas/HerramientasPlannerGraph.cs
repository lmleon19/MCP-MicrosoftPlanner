using System.ComponentModel;
using MCP.MicrosoftPlanner.Diagnostico;
using MCP.MicrosoftPlanner.Graph;
using Microsoft.Kiota.Abstractions;
using ModelContextProtocol.Server;

namespace MCP.MicrosoftPlanner.Herramientas;

/// <summary>Expone las consultas de solo lectura de Planner mediante herramientas MCP.</summary>
[McpServerToolType]
public sealed class HerramientasPlannerGraph(ServicioPlannerGraph servicioPlanner)
{
    private const int LongitudMaximaConsulta = 200;
    private const int MaximoResultadosBusqueda = 20;

    /// <summary>Obtiene la identidad de Microsoft Graph del usuario autenticado.</summary>
    [McpServerTool(Name = "obtener_identidad", ReadOnly = true)]
    [Description("Obtiene la identidad del usuario autenticado en Microsoft Graph.")]
    public Task<ResultadoHerramienta<IdentidadAutenticada>> ObtenerIdentidadAsync(CancellationToken cancellationToken)
        => EjecutarAsync(() => servicioPlanner.ObtenerIdentidadAsync(cancellationToken), "obtener identidad");

    /// <summary>Lista los planes accesibles por el usuario autenticado.</summary>
    [McpServerTool(Name = "listar_planes", ReadOnly = true)]
    [Description("Lista todos los planes Planner accesibles por el usuario autenticado.")]
    public Task<ResultadoHerramienta<IReadOnlyList<PlanPlanner>>> ListarPlanesAsync(CancellationToken cancellationToken)
        => EjecutarAsync(() => servicioPlanner.ListarPlanesAsync(cancellationToken), "listar planes");

    /// <summary>Obtiene un plan Planner básico por identificador.</summary>
    [McpServerTool(Name = "obtener_plan", ReadOnly = true)]
    [Description("Obtiene un plan Planner básico por su identificador.")]
    public Task<ResultadoHerramienta<PlanPlanner>> ObtenerPlanAsync(
        [Description("Identificador del plan Planner básico.")] string planId,
        CancellationToken cancellationToken)
        => EjecutarAsync(() => servicioPlanner.ObtenerPlanAsync(planId, cancellationToken), "obtener plan");

    /// <summary>Obtiene las etiquetas definidas en un plan Planner básico.</summary>
    [McpServerTool(Name = "obtener_detalles_plan", ReadOnly = true)]
    [Description("Obtiene las etiquetas definidas y el ETag de los detalles de un plan Planner básico.")]
    public Task<ResultadoHerramienta<DetallesPlanPlanner>> ObtenerDetallesPlanAsync(
        [Description("Identificador del plan Planner básico.")] string planId,
        CancellationToken cancellationToken)
        => EjecutarAsync(() => servicioPlanner.ObtenerDetallesPlanAsync(planId, cancellationToken), "obtener detalles de plan");

    /// <summary>Busca planes por coincidencia exacta de título.</summary>
    [McpServerTool(Name = "buscar_plan_por_nombre", ReadOnly = true)]
    [Description("Busca planes accesibles por nombre exacto y devuelve todas las coincidencias, sin seleccionar una ambigua.")]
    public Task<ResultadoHerramienta<ResultadoBusquedaPlanesPlanner>> BuscarPlanPorNombreAsync(
        [Description("Nombre exacto del plan Planner básico.")] string nombrePlan,
        CancellationToken cancellationToken)
        => EjecutarAsync(
            () => servicioPlanner.BuscarPlanesPorNombreAsync(nombrePlan, cancellationToken),
            "buscar plan por nombre");

    /// <summary>Lista los buckets de un plan Planner básico.</summary>
    [McpServerTool(Name = "listar_buckets", ReadOnly = true)]
    [Description("Lista los buckets de un plan Planner básico.")]
    public Task<ResultadoHerramienta<IReadOnlyList<BucketPlanner>>> ListarBucketsAsync(
        [Description("Identificador del plan Planner básico.")] string planId,
        CancellationToken cancellationToken)
        => EjecutarAsync(() => servicioPlanner.ListarBucketsAsync(planId, cancellationToken), "listar buckets");

    /// <summary>Lista las tareas de un plan Planner básico.</summary>
    [McpServerTool(Name = "listar_tareas", ReadOnly = true)]
    [Description("Lista las tareas de un plan Planner básico.")]
    public Task<ResultadoHerramienta<IReadOnlyList<TareaPlanner>>> ListarTareasAsync(
        [Description("Identificador del plan Planner básico.")] string planId,
        CancellationToken cancellationToken)
        => EjecutarAsync(() => servicioPlanner.ListarTareasAsync(planId, cancellationToken), "listar tareas");

    /// <summary>Obtiene una tarea Planner por identificador.</summary>
    [McpServerTool(Name = "obtener_tarea", ReadOnly = true)]
    [Description("Obtiene una tarea Planner por su identificador.")]
    public Task<ResultadoHerramienta<TareaPlanner>> ObtenerTareaAsync(
        [Description("Identificador de la tarea Planner.")] string tareaId,
        CancellationToken cancellationToken)
        => EjecutarAsync(() => servicioPlanner.ObtenerTareaAsync(tareaId, cancellationToken), "obtener tarea");

    /// <summary>Obtiene los detalles de una tarea Planner por identificador.</summary>
    [McpServerTool(Name = "obtener_detalle_tarea", ReadOnly = true)]
    [Description("Obtiene la descripción y el ETag de una tarea Planner.")]
    public Task<ResultadoHerramienta<DetalleTareaPlanner>> ObtenerDetalleTareaAsync(
        [Description("Identificador de la tarea Planner.")] string tareaId,
        CancellationToken cancellationToken)
        => EjecutarAsync(() => servicioPlanner.ObtenerDetalleTareaAsync(tareaId, cancellationToken), "obtener detalle de tarea");

    /// <summary>Busca texto controlado en los títulos de tareas de un plan.</summary>
    [McpServerTool(Name = "buscar_tareas", ReadOnly = true)]
    [Description("Busca texto controlado o un identificador externo incluido en títulos de tareas de un plan.")]
    public async Task<ResultadoHerramienta<IReadOnlyList<TareaPlanner>>> BuscarTareasAsync(
        [Description("Identificador del plan Planner básico.")] string planId,
        [Description("Texto o identificador externo a buscar en el título; máximo 200 caracteres.")] string consulta,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(consulta) || consulta.Length > LongitudMaximaConsulta)
        {
            return ResultadoHerramienta<IReadOnlyList<TareaPlanner>>.Fallo(
                new ErrorHerramienta("consulta_invalida", "La consulta debe contener entre 1 y 200 caracteres."));
        }

        return await EjecutarAsync(async () =>
        {
            var tareas = await servicioPlanner.ListarTareasAsync(planId, cancellationToken).ConfigureAwait(false);
            return (IReadOnlyList<TareaPlanner>)tareas
                .Where(tarea => tarea.Titulo?.Contains(consulta.Trim(), StringComparison.OrdinalIgnoreCase) == true)
                .Take(MaximoResultadosBusqueda)
                .ToArray();
        }, "buscar tareas").ConfigureAwait(false);
    }

    internal static async Task<ResultadoHerramienta<T>> EjecutarAsync<T>(Func<Task<T>> operacion, string nombreOperacion)
    {
        try
        {
            return ResultadoHerramienta<T>.Exito(await operacion().ConfigureAwait(false));
        }
        catch (ArgumentException)
        {
            return ResultadoHerramienta<T>.Fallo(
                new ErrorHerramienta("entrada_invalida", "Uno o más identificadores no son válidos."));
        }
        catch (InvalidOperationException exception)
        {
            // Las validaciones de entrada (confirmación, ETag y checklist) llegan aquí. No son
            // fallas de Graph, y reportarlas como error_graph inducía a reintentar sin corregir.
            RegistroSeguro.EscribirError($"La solicitud para {nombreOperacion} no cumple las validaciones de entrada.");
            return ResultadoHerramienta<T>.Fallo(
                new ErrorHerramienta("entrada_invalida", exception.Message));
        }
        catch (OperationCanceledException)
        {
            return ResultadoHerramienta<T>.Fallo(
                new ErrorHerramienta("cancelada", "La operación fue cancelada."));
        }
        catch (ApiException exception)
        {
            RegistroSeguro.EscribirError($"Microsoft Graph rechazó la operación {nombreOperacion}.");
            return ResultadoHerramienta<T>.Fallo(TraductorErroresGraph.Traducir(exception.ResponseStatusCode));
        }
        catch (Exception)
        {
            RegistroSeguro.EscribirError($"No fue posible {nombreOperacion} en Microsoft Graph.");
            return ResultadoHerramienta<T>.Fallo(
                new ErrorHerramienta("error_graph", "No fue posible completar la solicitud a Microsoft Graph."));
        }
    }
}
