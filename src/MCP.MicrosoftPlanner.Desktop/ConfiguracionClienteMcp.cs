using System.Text.Json;

namespace MCP.MicrosoftPlanner.Desktop;

public static class ConfiguracionClienteMcp
{
    public static string? EncontrarServidor(string? rutaEjecutable)
    {
        return rutaEjecutable is not null && File.Exists(rutaEjecutable)
            ? Path.GetFullPath(rutaEjecutable)
            : null;
    }

    public static string CrearConfiguracion(string rutaServidor)
        => JsonSerializer.Serialize(new
        {
            mcp = new
            {
                servers = new
                {
                    planner = new
                    {
                        type = "local",
                        command = new[] { rutaServidor, "--mcp" }
                    }
                }
            }
        }, new JsonSerializerOptions { WriteIndented = true });
}
