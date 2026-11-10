using Avalonia;
using MCP.MicrosoftPlanner;

namespace MCP.MicrosoftPlanner.Desktop;

internal static class Program
{
    [STAThread]
    public static async Task<int> Main(string[] args)
    {
        ModoInicio modo;
        try
        {
            modo = Inicio.Resolver(args);
        }
        catch (ArgumentException ex)
        {
            Console.Error.WriteLine(ex.Message);
            return 2;
        }

        if (modo == ModoInicio.Servidor)
        {
            await ServidorMcp.IniciarAsync([]);
            return 0;
        }

        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        return 0;
    }

    public static AppBuilder BuildAvaloniaApp() => AppBuilder.Configure<App>().UsePlatformDetect();
}
