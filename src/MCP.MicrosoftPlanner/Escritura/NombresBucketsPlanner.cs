namespace MCP.MicrosoftPlanner.Escritura;

/// <summary>Normaliza y compara nombres de buckets para operaciones idempotentes.</summary>
public static class NombresBucketsPlanner
{
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
}
