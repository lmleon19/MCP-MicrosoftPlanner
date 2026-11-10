namespace MCP.MicrosoftPlanner.Escritura;

/// <summary>Reglas reutilizables para una futura fase de escritura; no ejecuta operaciones Graph.</summary>
public static class GuardiaEscrituraPlanner
{
    /// <summary>Exige confirmación explícita antes de crear un recurso externo.</summary>
    public static void ValidarCreacion(bool confirmacionExplicita)
    {
        if (!confirmacionExplicita)
        {
            throw new InvalidOperationException("La operación mutante requiere confirmación explícita.");
        }
    }

    /// <summary>Exige confirmación explícita y un ETag válido antes de permitir una mutación.</summary>
    public static void Validar(bool confirmacionExplicita, string? eTag)
    {
        ValidarCreacion(confirmacionExplicita);

        if (string.IsNullOrWhiteSpace(eTag))
        {
            throw new InvalidOperationException("La operación mutante requiere un ETag válido.");
        }
    }
}
