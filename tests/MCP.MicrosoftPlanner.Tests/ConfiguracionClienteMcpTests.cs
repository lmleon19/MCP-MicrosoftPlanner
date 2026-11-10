using System.Text.Json;
using MCP.MicrosoftPlanner.Desktop;

namespace MCP.MicrosoftPlanner.Tests;

public sealed class ConfiguracionClienteMcpTests
{
    [Theory]
    [InlineData("MCP.MicrosoftPlanner.exe")]
    [InlineData("MCP.MicrosoftPlanner")]
    public void EncontrarServidor_UsaElMismoEjecutable(string nombre)
    {
        var raiz = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(raiz);
        var servidor = Path.Combine(raiz, nombre);
        try
        {
            File.WriteAllText(servidor, string.Empty);
            Assert.Equal(servidor, ConfiguracionClienteMcp.EncontrarServidor(servidor));
        }
        finally
        {
            Directory.Delete(raiz, true);
        }
    }

    [Fact]
    public void EncontrarServidor_SinEjecutableDevuelveNull()
    {
        var raiz = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(raiz);
        try
        {
            Assert.Null(ConfiguracionClienteMcp.EncontrarServidor(Path.Combine(raiz, "MCP.MicrosoftPlanner.exe")));
        }
        finally
        {
            Directory.Delete(raiz, true);
        }
    }

    [Fact]
    public void CrearConfiguracion_PreservaRutaConEspaciosYBarrasInvertidas()
    {
        const string ruta = @"C:\Program Files\Planner\MCP.MicrosoftPlanner.exe";
        using var documento = JsonDocument.Parse(ConfiguracionClienteMcp.CrearConfiguracion(ruta));
        var servidor = documento.RootElement.GetProperty("mcp").GetProperty("servers").GetProperty("planner");
        Assert.Equal("local", servidor.GetProperty("type").GetString());
        Assert.Equal(ruta, servidor.GetProperty("command")[0].GetString());
        Assert.Equal("--mcp", servidor.GetProperty("command")[1].GetString());
        Assert.Equal(2, servidor.GetProperty("command").GetArrayLength());
    }
}
