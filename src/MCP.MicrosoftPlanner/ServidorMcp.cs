using MCP.MicrosoftPlanner.Autenticacion;
using MCP.MicrosoftPlanner.Configuracion;
using MCP.MicrosoftPlanner.Graph;
using MCP.MicrosoftPlanner.Herramientas;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Protocol;

namespace MCP.MicrosoftPlanner;

public static class ServidorMcp
{
    public static async Task IniciarAsync(string[] args)
    {
        // MCP requires UTF-8 JSON on stdout, even when Windows starts with a legacy code page.
        Console.InputEncoding = new System.Text.UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
        Console.OutputEncoding = new System.Text.UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

        var builder = Host.CreateApplicationBuilder(args);
        builder.Logging.ClearProviders();
        var opciones = ConfiguracionPlannerGraph.CargarArchivos(
            Path.Combine(AppContext.BaseDirectory, "appsettings.json"),
            ConfiguracionPlannerGraph.RutaUsuario);
        builder.Services.AddSingleton(opciones);
        builder.Services.AddSingleton<ProveedorTokenMsal>();
        builder.Services.AddSingleton<ProveedorAutenticacionGraph>();
        builder.Services.AddSingleton<ServicioPlannerGraph>();
        builder.Services.AddSingleton<HerramientasPlannerGraph>();
        builder.Services.AddSingleton<HerramientasEscrituraPlannerGraph>();
        var servidorMcp = builder.Services
            .AddMcpServer(options => options.ServerInfo = new Implementation
            {
                Name = typeof(ServidorMcp).Assembly.GetName().Name!,
                Version = typeof(ServidorMcp).Assembly.GetName().Version!.ToString()
            })
            .WithStdioServerTransport()
            .WithTools<HerramientasPlannerGraph>();

        if (!opciones.SoloLectura)
        {
            // Read-only mode must neither request write scopes nor expose write tools.
            servidorMcp.WithTools<HerramientasEscrituraPlannerGraph>();
        }

        await builder.Build().RunAsync();
    }
}
