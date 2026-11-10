namespace MCP.MicrosoftPlanner.Escritura;

/// <summary>Fusiona categorías y asignaciones de una tarea sin perder el estado actual.</summary>
public static class FusionTareaPlanner
{
    /// <summary>Orden de asignación usado para asignados nuevos cuando Graph no provee uno.</summary>
    public const string OrdenAsignacionNuevo = " !";

    /// <summary>Marca o desmarca claves solicitadas conservando las demás categorías.</summary>
    public static IReadOnlyDictionary<string, bool> FusionarCategorias(
        IEnumerable<KeyValuePair<string, bool>>? existentes,
        IEnumerable<string>? solicitadas,
        IEnumerable<string>? desmarcadas = null)
    {
        var fusionadas = new Dictionary<string, bool>(StringComparer.Ordinal);
        if (existentes is not null)
        {
            foreach (var par in existentes)
            {
                if (string.IsNullOrWhiteSpace(par.Key))
                {
                    continue;
                }

                fusionadas[par.Key] = par.Value;
            }
        }

        if (solicitadas is not null)
        {
            foreach (var clave in solicitadas)
            {
                AplicarCategoria(fusionadas, clave, true, nameof(solicitadas));
            }
        }

        if (desmarcadas is not null)
        {
            foreach (var clave in desmarcadas)
            {
                AplicarCategoria(fusionadas, clave, false, nameof(desmarcadas));
            }
        }

        return fusionadas;
    }

    /// <summary>Obtiene los usuarios solicitados que aún no están asignados.</summary>
    public static IReadOnlyList<string> ObtenerAsignadosFaltantes(
        IEnumerable<string>? existentes,
        IEnumerable<string>? solicitados)
    {
        var existentesNormalizados = (existentes ?? [])
            .Where(idUsuario => !string.IsNullOrWhiteSpace(idUsuario))
            .Select(idUsuario => idUsuario.Trim())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        return (solicitados ?? [])
            .Where(idUsuario => !string.IsNullOrWhiteSpace(idUsuario))
            .Select(idUsuario => idUsuario.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Where(idUsuario => !existentesNormalizados.Contains(idUsuario))
            .ToArray();
    }

    private static void AplicarCategoria(
        IDictionary<string, bool> categorias,
        string? clave,
        bool aplicada,
        string nombreParametro)
    {
        if (string.IsNullOrWhiteSpace(clave))
        {
            return;
        }

        if (!NombresEtiquetasPlanner.EsClaveCategoria(clave))
        {
            throw new ArgumentException(
                $"La clave de categoría '{clave}' no es válida (category1..{NombresEtiquetasPlanner.MaximoEtiquetasPorPlan}).",
                nombreParametro);
        }

        categorias[clave.Trim()] = aplicada;
    }
}
