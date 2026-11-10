namespace MCP.MicrosoftPlanner.Escritura;

/// <summary>Normaliza, valida y asigna claves de etiquetas de un plan Planner básico.</summary>
public static class NombresEtiquetasPlanner
{
    public const int MaximoEtiquetasPorPlan = 25;

    /// <summary>Obtiene los nombres solicitados que no están presentes entre los existentes.</summary>
    public static IReadOnlyList<string> ObtenerFaltantes(
        IEnumerable<string?> existentes,
        IEnumerable<string?> solicitados)
    {
        ArgumentNullException.ThrowIfNull(existentes);
        ArgumentNullException.ThrowIfNull(solicitados);

        var existentesNormalizados = existentes
            .Where(nombre => !string.IsNullOrWhiteSpace(nombre))
            .Select(nombre => nombre!.Trim())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        return solicitados
            .Where(nombre => !string.IsNullOrWhiteSpace(nombre))
            .Select(nombre => nombre!.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Where(nombre => !existentesNormalizados.Contains(nombre))
            .ToArray();
    }

    /// <summary>
    /// Asigna a cada etiqueta solicitada y faltante el primer hueco libre (category1..category25).
    /// Reutiliza las claves ya usadas en <paramref name="existentes"/> y conserva las etiquetas existentes.
    /// </summary>
    public static IReadOnlyDictionary<string, string> AsignarClavesLibres(
        IReadOnlyDictionary<string, string> existentes,
        IEnumerable<string?> solicitados)
    {
        ArgumentNullException.ThrowIfNull(existentes);
        ArgumentNullException.ThrowIfNull(solicitados);

        var clavesUsadas = existentes.Keys
            .Where(EsClaveCategoria)
            .ToHashSet(StringComparer.Ordinal);
        var nombresExistentes = existentes
            .Where(par => !string.IsNullOrWhiteSpace(par.Value))
            .Select(par => par.Value.Trim())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var asignadas = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var nombre in solicitados
                     .Where(nombre => !string.IsNullOrWhiteSpace(nombre))
                     .Select(nombre => nombre!.Trim())
                     .Distinct(StringComparer.OrdinalIgnoreCase))
        {
            if (nombresExistentes.Contains(nombre) || asignadas.ContainsKey(nombre))
            {
                continue;
            }

            var clave = EncontrarClaveLibre(clavesUsadas);
            clavesUsadas.Add(clave);
            asignadas[nombre] = clave;
        }

        return asignadas;
    }

    /// <summary>Indica si la clave es una categoría válida (category1..category25).</summary>
    public static bool EsClaveCategoria(string? clave) => ObtenerIndiceCategoria(clave) >= 1;

    /// <summary>Obtiene el índice numérico de una clave de categoría o -1 si no es válida.</summary>
    public static int ObtenerIndiceCategoria(string? clave)
    {
        if (string.IsNullOrWhiteSpace(clave) || !clave.StartsWith("category", StringComparison.Ordinal))
        {
            return -1;
        }

        var sufijo = clave["category".Length..];
        return int.TryParse(sufijo, out var indice) && indice is >= 1 and <= MaximoEtiquetasPorPlan
            ? indice
            : -1;
    }

    private static string EncontrarClaveLibre(IReadOnlySet<string> clavesUsadas)
    {
        for (var indice = 1; indice <= MaximoEtiquetasPorPlan; indice++)
        {
            var clave = $"category{indice}";
            if (!clavesUsadas.Contains(clave))
            {
                return clave;
            }
        }

        throw new InvalidOperationException(
            $"El plan ya tiene {MaximoEtiquetasPorPlan} etiquetas y no admite más.");
    }
}
