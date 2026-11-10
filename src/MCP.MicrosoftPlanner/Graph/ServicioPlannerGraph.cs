using MCP.MicrosoftPlanner.Autenticacion;
using MCP.MicrosoftPlanner.Escritura;
using Microsoft.Graph;
using Microsoft.Graph.Models;

namespace MCP.MicrosoftPlanner.Graph;

/// <summary>Consulta recursos Planner básicos mediante Microsoft Graph v1.0.</summary>
public sealed class ServicioPlannerGraph
{
    private const int LongitudMaximaTituloChecklist = 100;
    private readonly Lazy<GraphServiceClient> _cliente;

    public ServicioPlannerGraph(ProveedorAutenticacionGraph proveedorAutenticacion)
    {
        _cliente = new Lazy<GraphServiceClient>(() => new GraphServiceClient(
            proveedorAutenticacion,
            "https://graph.microsoft.com/v1.0"));
    }

    /// <summary>Inicializa el servicio con un cliente Graph ya configurado.</summary>
    internal ServicioPlannerGraph(GraphServiceClient cliente)
    {
        ArgumentNullException.ThrowIfNull(cliente);
        _cliente = new Lazy<GraphServiceClient>(() => cliente);
    }

    /// <summary>Obtiene la identidad del usuario autenticado.</summary>
    public async Task<IdentidadAutenticada> ObtenerIdentidadAsync(CancellationToken cancellationToken)
    {
        var usuario = await _cliente.Value.Me.GetAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
        return new IdentidadAutenticada(usuario?.Id, usuario?.DisplayName, usuario?.UserPrincipalName);
    }

    /// <summary>Obtiene un plan básico por identificador.</summary>
    public async Task<PlanPlanner> ObtenerPlanAsync(string planId, CancellationToken cancellationToken)
    {
        ValidarIdentificador(planId, nameof(planId));
        var plan = await _cliente.Value.Planner.Plans[planId].GetAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
        return Mapear(plan);
    }

    /// <summary>Obtiene las etiquetas y el ETag de los detalles de un plan Planner básico.</summary>
    public async Task<DetallesPlanPlanner> ObtenerDetallesPlanAsync(string planId, CancellationToken cancellationToken)
    {
        ValidarIdentificador(planId, nameof(planId));
        var detalle = await _cliente.Value.Planner.Plans[planId].Details
            .GetAsync(cancellationToken: cancellationToken)
            .ConfigureAwait(false);
        return Mapear(detalle);
    }

    /// <summary>Busca los planes accesibles cuyo título coincide exactamente con el nombre indicado.</summary>
    public async Task<ResultadoBusquedaPlanesPlanner> BuscarPlanesPorNombreAsync(
        string nombrePlan,
        CancellationToken cancellationToken)
    {
        ValidarNombre(nombrePlan, nameof(nombrePlan));
        var planes = await ListarPlanesAsync(cancellationToken).ConfigureAwait(false);
        var nombreNormalizado = nombrePlan.Trim();
        var coincidencias = planes
            .Where(plan => NombresPlanesPlanner.CoincideExactamente(plan.Titulo, nombreNormalizado))
            .ToArray();
        return new ResultadoBusquedaPlanesPlanner(nombreNormalizado, coincidencias);
    }

    /// <summary>Lista los buckets de un plan básico.</summary>
    public async Task<IReadOnlyList<BucketPlanner>> ListarBucketsAsync(string planId, CancellationToken cancellationToken)
    {
        ValidarIdentificador(planId, nameof(planId));
        var buckets = new List<BucketPlanner>();
        var respuesta = await _cliente.Value.Planner.Plans[planId].Buckets.GetAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
        while (respuesta is not null)
        {
            buckets.AddRange(respuesta.Value?.Select(Mapear) ?? []);
            if (string.IsNullOrWhiteSpace(respuesta.OdataNextLink))
            {
                break;
            }

            respuesta = await _cliente.Value.Planner.Plans[planId].Buckets
                .WithUrl(respuesta.OdataNextLink)
                .GetAsync(cancellationToken: cancellationToken)
                .ConfigureAwait(false);
        }

        return buckets;
    }

    /// <summary>Lista los planes accesibles por el usuario autenticado.</summary>
    public async Task<IReadOnlyList<PlanPlanner>> ListarPlanesAsync(CancellationToken cancellationToken)
    {
        var respuesta = await _cliente.Value.Me.Planner.Plans.GetAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
        var planes = new List<PlanPlanner>();
        while (respuesta is not null)
        {
            planes.AddRange(respuesta.Value?.Select(Mapear) ?? []);
            if (string.IsNullOrWhiteSpace(respuesta.OdataNextLink))
            {
                break;
            }

            respuesta = await _cliente.Value.Me.Planner.Plans
                .WithUrl(respuesta.OdataNextLink)
                .GetAsync(cancellationToken: cancellationToken)
                .ConfigureAwait(false);
        }

        return planes;
    }

    /// <summary>Lista las tareas de un plan básico.</summary>
    public async Task<IReadOnlyList<TareaPlanner>> ListarTareasAsync(string planId, CancellationToken cancellationToken)
    {
        ValidarIdentificador(planId, nameof(planId));
        var tareas = new List<TareaPlanner>();
        var respuesta = await _cliente.Value.Planner.Plans[planId].Tasks.GetAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
        while (respuesta is not null)
        {
            tareas.AddRange(respuesta.Value?.Select(Mapear) ?? []);
            if (string.IsNullOrWhiteSpace(respuesta.OdataNextLink))
            {
                break;
            }

            respuesta = await _cliente.Value.Planner.Plans[planId].Tasks
                .WithUrl(respuesta.OdataNextLink)
                .GetAsync(cancellationToken: cancellationToken)
                .ConfigureAwait(false);
        }

        return tareas;
    }

    /// <summary>Obtiene una tarea por identificador.</summary>
    public async Task<TareaPlanner> ObtenerTareaAsync(string tareaId, CancellationToken cancellationToken)
    {
        ValidarIdentificador(tareaId, nameof(tareaId));
        var tarea = await _cliente.Value.Planner.Tasks[tareaId].GetAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
        return Mapear(tarea);
    }

    /// <summary>Obtiene los detalles de una tarea por identificador.</summary>
    public async Task<DetalleTareaPlanner> ObtenerDetalleTareaAsync(string tareaId, CancellationToken cancellationToken)
    {
        ValidarIdentificador(tareaId, nameof(tareaId));
        var detalle = await _cliente.Value.Planner.Tasks[tareaId].Details.GetAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
        return Mapear(detalle);
    }

    /// <summary>Crea un bucket en un plan Planner básico.</summary>
    public async Task<BucketPlanner> CrearBucketAsync(string planId, string nombre, CancellationToken cancellationToken)
    {
        ValidarIdentificador(planId, nameof(planId));
        ValidarNombre(nombre, nameof(nombre));
        var bucket = await _cliente.Value.Planner.Buckets.PostAsync(
            new PlannerBucket { PlanId = planId, Name = nombre.Trim() },
            cancellationToken: cancellationToken).ConfigureAwait(false);
        return Mapear(bucket);
    }

    /// <summary>Crea una tarea en el bucket indicado.</summary>
    public async Task<TareaPlanner> CrearTareaAsync(string planId, string bucketId, string titulo, CancellationToken cancellationToken)
    {
        ValidarIdentificador(planId, nameof(planId));
        ValidarIdentificador(bucketId, nameof(bucketId));
        ValidarNombre(titulo, nameof(titulo));
        var tarea = await _cliente.Value.Planner.Tasks.PostAsync(
            new PlannerTask { PlanId = planId, BucketId = bucketId, Title = titulo.Trim() },
            cancellationToken: cancellationToken).ConfigureAwait(false);
        return Mapear(tarea);
    }

    /// <summary>Asegura etiquetas del plan sin crear duplicados y usando los primeros huecos libres.</summary>
    public async Task<DetallesPlanPlanner> AsegurarEtiquetasPlanAsync(
        string planId,
        IReadOnlyList<string> nombres,
        CancellationToken cancellationToken)
    {
        ValidarIdentificador(planId, nameof(planId));
        ArgumentNullException.ThrowIfNull(nombres);

        var detalle = await _cliente.Value.Planner.Plans[planId].Details
            .GetAsync(cancellationToken: cancellationToken)
            .ConfigureAwait(false);
        var existentes = ObtenerDescripcionesCategorias(detalle?.CategoryDescriptions);
        var faltantes = NombresEtiquetasPlanner.ObtenerFaltantes(existentes.Values, nombres);
        if (faltantes.Count == 0)
        {
            return Mapear(detalle);
        }

        var nuevasClaves = NombresEtiquetasPlanner.AsignarClavesLibres(existentes, faltantes);
        var fusionadas = new Dictionary<string, string>(existentes, StringComparer.Ordinal);
        foreach (var nuevaEtiqueta in nuevasClaves)
        {
            fusionadas[nuevaEtiqueta.Value] = nuevaEtiqueta.Key;
        }

        var eTag = ObtenerETag(detalle) ?? throw new InvalidOperationException("No se obtuvo un ETag para actualizar el plan.");
        var actualizado = await _cliente.Value.Planner.Plans[planId].Details.PatchAsync(
            new PlannerPlanDetails { CategoryDescriptions = CrearDescripcionesCategorias(fusionadas) },
            configuracion => configuracion.Headers.Add("If-Match", eTag),
            cancellationToken).ConfigureAwait(false);
        return Mapear(actualizado);
    }

    /// <summary>Actualiza campos básicos de una tarea mediante su ETag.</summary>
    public async Task<TareaPlanner> ActualizarTareaAsync(
        string tareaId,
        string eTag,
        ActualizacionTareaPlanner actualizacion,
        CancellationToken cancellationToken)
    {
        ValidarIdentificador(tareaId, nameof(tareaId));
        ValidarETag(eTag);
        ArgumentNullException.ThrowIfNull(actualizacion);
        var solicitud = new PlannerTask();
        if (actualizacion.Titulo is not null)
        {
            solicitud.Title = actualizacion.Titulo.Trim();
        }

        if (actualizacion.BucketId is not null)
        {
            solicitud.BucketId = actualizacion.BucketId.Trim();
        }

        if (actualizacion.PorcentajeCompletado is not null)
        {
            solicitud.PercentComplete = actualizacion.PorcentajeCompletado;
        }

        if (actualizacion.FechaInicio is not null)
        {
            solicitud.StartDateTime = actualizacion.FechaInicio;
        }

        if (actualizacion.FechaVencimiento is not null)
        {
            solicitud.DueDateTime = actualizacion.FechaVencimiento;
        }

        if (actualizacion.Categorias is not null ||
            actualizacion.CategoriasDesmarcadas is not null ||
            actualizacion.Asignaciones is not null)
        {
            var tareaActual = await _cliente.Value.Planner.Tasks[tareaId]
                .GetAsync(cancellationToken: cancellationToken)
                .ConfigureAwait(false);

            if (actualizacion.Categorias is not null || actualizacion.CategoriasDesmarcadas is not null)
            {
                var categoriasExistentes = tareaActual?.AppliedCategories?.AdditionalData?
                    .Where(categoria => categoria.Value is bool)
                    .Select(categoria => new KeyValuePair<string, bool>(categoria.Key, (bool)categoria.Value))
                    .ToArray();
                solicitud.AppliedCategories = new PlannerAppliedCategories
                {
                    AdditionalData = FusionTareaPlanner
                        .FusionarCategorias(
                            categoriasExistentes,
                            actualizacion.Categorias,
                            actualizacion.CategoriasDesmarcadas)
                        .ToDictionary(par => par.Key, par => (object)par.Value, StringComparer.Ordinal),
                };
            }

            if (actualizacion.Asignaciones is not null)
            {
                var asignaciones = tareaActual?.Assignments?.AdditionalData is null
                    ? new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
                    : new Dictionary<string, object>(
                        tareaActual.Assignments.AdditionalData,
                        StringComparer.OrdinalIgnoreCase);
                var asignadosFaltantes = FusionTareaPlanner.ObtenerAsignadosFaltantes(
                    asignaciones.Keys,
                    actualizacion.Asignaciones);
                foreach (var idUsuario in asignadosFaltantes)
                {
                    asignaciones[idUsuario] = new PlannerAssignment
                    {
                        OdataType = "#microsoft.graph.plannerAssignment",
                        OrderHint = FusionTareaPlanner.OrdenAsignacionNuevo,
                    };
                }

                if (asignadosFaltantes.Count > 0)
                {
                    solicitud.Assignments = new PlannerAssignments
                    {
                        AdditionalData = asignaciones.ToDictionary(
                            asignacion => asignacion.Key,
                            asignacion => asignacion.Value,
                            StringComparer.Ordinal),
                    };
                }
            }
        }

        var tarea = await _cliente.Value.Planner.Tasks[tareaId].PatchAsync(
            solicitud,
            configuracion => configuracion.Headers.Add("If-Match", eTag),
            cancellationToken).ConfigureAwait(false);
        return Mapear(tarea);
    }

    /// <summary>Actualiza descripción y elementos de checklist sin eliminar elementos existentes.</summary>
    public async Task<DetalleTareaPlanner> ActualizarDetalleTareaAsync(
        string tareaId,
        string eTag,
        string? descripcion,
        IReadOnlyList<ElementoChecklistPlanner>? checklist,
        CancellationToken cancellationToken)
    {
        ValidarIdentificador(tareaId, nameof(tareaId));
        ValidarETag(eTag);
        ValidarChecklist(checklist);
        var detalle = new PlannerTaskDetails { Description = descripcion };
        if (checklist is { Count: > 0 })
        {
            detalle.Checklist = ChecklistTareasPlanner.Crear(checklist);
        }

        var respuesta = await _cliente.Value.Planner.Tasks[tareaId].Details.PatchAsync(
            detalle,
            configuracion =>
            {
                configuracion.Headers.Add("If-Match", eTag);
                configuracion.Headers.Add("Prefer", "return=representation");
            },
            cancellationToken).ConfigureAwait(false);
        return Mapear(respuesta);
    }

    /// <summary>Elimina una tarea mediante su ETag, que Graph exige como control de concurrencia.</summary>
    public async Task EliminarTareaAsync(string tareaId, string eTag, CancellationToken cancellationToken)
    {
        ValidarIdentificador(tareaId, nameof(tareaId));
        ValidarETag(eTag);
        await _cliente.Value.Planner.Tasks[tareaId].DeleteAsync(
            configuracion => configuracion.Headers.Add("If-Match", eTag),
            cancellationToken).ConfigureAwait(false);
    }

    private static PlanPlanner Mapear(PlannerPlan? plan) => new(plan?.Id, plan?.Title, ObtenerETag(plan));

    private static BucketPlanner Mapear(PlannerBucket? bucket) => new(bucket?.Id, bucket?.Name, bucket?.PlanId, ObtenerETag(bucket));

    private static DetalleTareaPlanner Mapear(PlannerTaskDetails? detalle) => new(
        detalle?.Description,
        ObtenerETag(detalle),
        ChecklistTareasPlanner.Mapear(detalle?.Checklist));

    private static TareaPlanner Mapear(PlannerTask? tarea) => new(
        tarea?.Id,
        tarea?.Title,
        tarea?.PlanId,
        tarea?.BucketId,
        tarea?.PercentComplete,
        tarea?.DueDateTime,
        ObtenerETag(tarea),
        tarea?.AppliedCategories?.AdditionalData?
            .Where(categoria => categoria.Value is true)
            .Select(categoria => categoria.Key)
            .OrderBy(clave => clave, StringComparer.Ordinal)
            .ToArray(),
        tarea?.Assignments?.AdditionalData?
            .Select(asignacion => asignacion.Key)
            .OrderBy(idUsuario => idUsuario, StringComparer.Ordinal)
            .ToArray(),
        tarea?.StartDateTime);

    private static DetallesPlanPlanner Mapear(PlannerPlanDetails? detalle) => new(
        ObtenerDescripcionesCategorias(detalle?.CategoryDescriptions)
            .Select(etiqueta => new EtiquetaPlanPlanner(etiqueta.Key, etiqueta.Value.Trim()))
            .OrderBy(etiqueta => NombresEtiquetasPlanner.ObtenerIndiceCategoria(etiqueta.Clave))
            .ThenBy(etiqueta => etiqueta.Clave, StringComparer.Ordinal)
            .ToArray(),
        ObtenerETag(detalle));

    private static Dictionary<string, string> ObtenerDescripcionesCategorias(PlannerCategoryDescriptions? descripciones)
    {
        var resultado = new Dictionary<string, string>(StringComparer.Ordinal);
        if (descripciones is null)
        {
            return resultado;
        }

        for (var indice = 1; indice <= NombresEtiquetasPlanner.MaximoEtiquetasPorPlan; indice++)
        {
            var propiedad = typeof(PlannerCategoryDescriptions).GetProperty($"Category{indice}");
            if (propiedad?.GetValue(descripciones) is string nombre && !string.IsNullOrWhiteSpace(nombre))
            {
                resultado[$"category{indice}"] = nombre.Trim();
            }
        }

        return resultado;
    }

    private static PlannerCategoryDescriptions CrearDescripcionesCategorias(IReadOnlyDictionary<string, string> etiquetas)
    {
        var descripciones = new PlannerCategoryDescriptions();
        foreach (var etiqueta in etiquetas)
        {
            var indice = NombresEtiquetasPlanner.ObtenerIndiceCategoria(etiqueta.Key);
            if (indice < 1 || string.IsNullOrWhiteSpace(etiqueta.Value))
            {
                continue;
            }

            typeof(PlannerCategoryDescriptions)
                .GetProperty($"Category{indice}")!
                .SetValue(descripciones, etiqueta.Value.Trim());
        }

        return descripciones;
    }

    private static string? ObtenerETag(Microsoft.Kiota.Abstractions.Serialization.IAdditionalDataHolder? recurso) =>
        recurso?.AdditionalData.TryGetValue("@odata.etag", out var valor) == true ? valor as string : null;

    private static void ValidarIdentificador(string identificador, string nombreParametro)
    {
        if (string.IsNullOrWhiteSpace(identificador))
        {
            throw new ArgumentException("El identificador es obligatorio.", nombreParametro);
        }
    }

    private static void ValidarChecklist(IReadOnlyList<ElementoChecklistPlanner>? checklist)
    {
        if (checklist is null)
        {
            return;
        }

        var ids = new HashSet<string>(StringComparer.Ordinal);
        foreach (var elemento in checklist)
        {
            if (!Guid.TryParse(elemento.Id, out _))
            {
                throw new InvalidOperationException("El identificador de un elemento de checklist no es válido.");
            }

            if (!ids.Add(elemento.Id))
            {
                throw new InvalidOperationException("Existen identificadores duplicados en el checklist.");
            }

            if (elemento.Titulo is null)
            {
                continue;
            }

            if (string.IsNullOrWhiteSpace(elemento.Titulo) || elemento.Titulo.Length > LongitudMaximaTituloChecklist)
            {
                throw new InvalidOperationException($"El título de un elemento de checklist debe tener entre 1 y {LongitudMaximaTituloChecklist} caracteres.");
            }
        }
    }

    private static void ValidarNombre(string nombre, string nombreParametro)
    {
        if (string.IsNullOrWhiteSpace(nombre))
        {
            throw new ArgumentException("El nombre es obligatorio.", nombreParametro);
        }
    }

    private static void ValidarETag(string eTag)
    {
        if (string.IsNullOrWhiteSpace(eTag))
        {
            throw new ArgumentException("El ETag es obligatorio.", nameof(eTag));
        }
    }
}
