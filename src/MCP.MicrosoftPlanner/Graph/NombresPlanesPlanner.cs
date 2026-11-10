namespace MCP.MicrosoftPlanner.Graph;

/// <summary>Compara títulos de planes sin seleccionar coincidencias parciales.</summary>
public static class NombresPlanesPlanner
{
    /// <summary>Indica si un título coincide exactamente con el nombre solicitado tras normalizar espacios externos.</summary>
    public static bool CoincideExactamente(string? titulo, string nombrePlan) =>
        !string.IsNullOrWhiteSpace(titulo) &&
        string.Equals(titulo.Trim(), nombrePlan.Trim(), StringComparison.OrdinalIgnoreCase);
}
