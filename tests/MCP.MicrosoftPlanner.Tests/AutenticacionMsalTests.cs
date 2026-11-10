using MCP.MicrosoftPlanner.Autenticacion;
using MCP.MicrosoftPlanner.Configuracion;
using Microsoft.Identity.Client;

namespace MCP.MicrosoftPlanner.Tests;

public sealed class AutenticacionMsalTests
{
    [Fact]
    public void RedirectUri_UsaLoopbackSinPuertoFijo()
    {
        var redirectUri = new Uri(OpcionesInteractiveMicrosoft.RedirectUri);

        Assert.True(redirectUri.IsLoopback);
        Assert.True(redirectUri.IsDefaultPort);
        Assert.DoesNotContain(":", redirectUri.Authority, StringComparison.Ordinal);
    }

    [Fact]
    public void Opciones_CuandoSeConstruyen_DefinenMensajesParaElNavegador()
    {
        var opciones = OpcionesInteractiveMicrosoft.CrearOpciones();

        Assert.False(string.IsNullOrWhiteSpace(opciones.HtmlMessageSuccess));
        Assert.False(string.IsNullOrWhiteSpace(opciones.HtmlMessageError));
    }

    [Fact]
    public async Task ObtenerToken_CuandoLaSesionSilenciosaFunciona_NoAbreElNavegador()
    {
        var repositorio = new RepositorioTokenSimulado { TokenSilencioso = "token-silencioso" };
        var proveedor = new ProveedorTokenMsal(new ConfiguracionPlannerGraph("tenant", "client"), repositorio);

        var token = await proveedor.ObtenerTokenAsync(CancellationToken.None);

        Assert.Equal("token-silencioso", token);
        Assert.False(repositorio.UsarInteractivo);
    }

    [Fact]
    public async Task ObtenerToken_CuandoSeRequiereInteraccion_UsaElFlujoInteractivo()
    {
        var repositorio = new RepositorioTokenSimulado
        {
            ErrorSilencioso = new MsalUiRequiredException("requiere interaccion", Guid.NewGuid().ToString()),
        };
        var proveedor = new ProveedorTokenMsal(new ConfiguracionPlannerGraph("tenant", "client"), repositorio);

        var token = await proveedor.ObtenerTokenAsync(CancellationToken.None);

        Assert.Equal("token-interactivo", token);
        Assert.True(repositorio.UsarInteractivo);
    }

    [Fact]
    public async Task ObtenerToken_CuandoLaInteraccionFalla_PropagaElError()
    {
        var repositorio = new RepositorioTokenSimulado
        {
            ErrorSilencioso = new MsalUiRequiredException("requiere interaccion", Guid.NewGuid().ToString()),
            ErrorInteractivo = new InvalidOperationException("interaccion rechazada"),
        };
        var proveedor = new ProveedorTokenMsal(new ConfiguracionPlannerGraph("tenant", "client"), repositorio);

        await Assert.ThrowsAsync<InvalidOperationException>(() => proveedor.ObtenerTokenAsync(CancellationToken.None));
    }

    private sealed class RepositorioTokenSimulado : IRepositorioTokenMsal
    {
        public string TokenSilencioso { get; init; } = "token-silencioso";

        public Exception? ErrorSilencioso { get; init; }

        public Exception? ErrorInteractivo { get; init; }

        public bool UsarInteractivo { get; private set; }

        public Task<string> ObtenerTokenSilenciosoAsync(CancellationToken cancellationToken) =>
            ErrorSilencioso is null
                ? Task.FromResult(TokenSilencioso)
                : Task.FromException<string>(ErrorSilencioso);

        public Task<string> ObtenerTokenInteractivoAsync(CancellationToken cancellationToken)
        {
            UsarInteractivo = true;
            return ErrorInteractivo is null
                ? Task.FromResult("token-interactivo")
                : Task.FromException<string>(ErrorInteractivo);
        }
    }
}
