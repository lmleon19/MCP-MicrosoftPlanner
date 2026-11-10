using MCP.MicrosoftPlanner.Desktop;

namespace MCP.MicrosoftPlanner.Tests;

public sealed class ModoInicioTests
{
    [Fact]
    public void Resolver_SinArgumentosAbreConfigurador()
        => Assert.Equal(ModoInicio.Configurador, Inicio.Resolver([]));

    [Fact]
    public void Resolver_ConMcpIniciaServidor()
        => Assert.Equal(ModoInicio.Servidor, Inicio.Resolver(["--mcp"]));

    [Theory]
    [InlineData("--desconocido")]
    [InlineData("--mcp --otro")]
    public void Resolver_ArgumentosNoSoportadosFalla(string argumentos)
        => Assert.Throws<ArgumentException>(() => Inicio.Resolver(argumentos.Split(' ')));
}
