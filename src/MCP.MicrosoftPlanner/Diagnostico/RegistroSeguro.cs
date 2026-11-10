using System.Text.RegularExpressions;

namespace MCP.MicrosoftPlanner.Diagnostico;

/// <summary>Centraliza la sanitización de diagnósticos enviados exclusivamente a stderr.</summary>
public static partial class RegistroSeguro
{
    /// <summary>Escribe un diagnóstico seguro en stderr, nunca en stdout.</summary>
    public static void EscribirError(string mensaje)
    {
        Console.Error.WriteLine(Sanitizar(mensaje));
    }

    /// <summary>Elimina valores de autorización y tokens de un diagnóstico.</summary>
    public static string Sanitizar(string mensaje)
    {
        var sinBearer = BearerRegex().Replace(mensaje, "Bearer [REDACTADO]");
        return AuthorizationRegex().Replace(sinBearer, "Authorization: [REDACTADO]");
    }

    [GeneratedRegex("Bearer\\s+[^\\s]+", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex BearerRegex();

    [GeneratedRegex("Authorization\\s*:\\s*[^\\r\\n]+", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex AuthorizationRegex();
}
