namespace MCP.MicrosoftPlanner.Desktop;

public enum ModoInicio
{
    Configurador,
    Servidor
}

public static class Inicio
{
    public static ModoInicio Resolver(string[] args)
    {
        if (args.Length == 0) return ModoInicio.Configurador;
        if (args.Length == 1 && args[0] == "--mcp") return ModoInicio.Servidor;
        throw new ArgumentException("Usage: MCP.MicrosoftPlanner [--mcp]");
    }
}
