using System.Net;
using System.Reflection;
using MCP.MicrosoftPlanner.Graph;
using MCP.MicrosoftPlanner.Herramientas;
using Microsoft.Graph;
using Microsoft.Kiota.Abstractions.Authentication;

namespace MCP.MicrosoftPlanner.Tests;

public sealed class CatalogoHerramientasTests
{
    [Fact]
    public void Herramientas_CuandoSeDeclaran_NoRepitenElPrefijoDelServidor()
    {
        var nombres = ObtenerNombres();

        Assert.DoesNotContain(nombres, nombre => nombre.StartsWith("planner_", StringComparison.Ordinal));
        Assert.Equal(nombres.Count, nombres.Distinct(StringComparer.Ordinal).Count());
    }

    [Fact]
    public void Herramientas_CuandoSeDeclaran_ExponeListarPlanesYEliminarTarea()
    {
        var nombres = ObtenerNombres();

        Assert.Contains("listar_planes", nombres);
        Assert.Contains("eliminar_tarea", nombres);
    }

    [Fact]
    public void Herramientas_DeEscritura_CuandoSeDeclaran_ExigenConfirmarYETag()
    {
        var herramientas = ObtenerPropiedadesDeHerramientas();
        var (soloLectura, parametros) = herramientas["eliminar_tarea"];

        Assert.False(soloLectura);
        Assert.Contains("confirmar", parametros);
        Assert.Contains("eTag", parametros);
    }

    [Fact]
    public void Herramientas_DeLectura_CuandoSeDeclaran_NoExigenConfirmar()
    {
        var herramientas = ObtenerPropiedadesDeHerramientas();
        var (soloLectura, parametros) = herramientas["listar_planes"];

        Assert.True(soloLectura);
        Assert.DoesNotContain("confirmar", parametros);
    }

    [Fact]
    public async Task EliminarTarea_CuandoFaltaConfirmacion_NoInvocaGraph()
    {
        var manejador = new ManejadorGraphSimulado(_ => new HttpResponseMessage(HttpStatusCode.NoContent));
        var servicio = new ServicioPlannerGraph(
            new GraphServiceClient(
                new HttpClient(manejador) { BaseAddress = new Uri("https://graph.microsoft.com/v1.0") },
                new AnonymousAuthenticationProvider(),
                "https://graph.microsoft.com/v1.0"));
        var herramienta = new HerramientasEscrituraPlannerGraph(servicio);

        var resultado = await herramienta.EliminarTareaAsync("t1", "etag", confirmar: false, CancellationToken.None);

        Assert.Null(resultado.Datos);
        Assert.Equal("entrada_invalida", resultado.Error!.Codigo);
        Assert.Empty(manejador.Solicitudes);
    }

    private static List<string> ObtenerNombres() =>
    [
        .. Declaradas().Select(propiedad => LeerAtributo(propiedad)!.GetType().GetProperty("Name")!.GetValue(LeerAtributo(propiedad)) as string ?? string.Empty),
    ];

    private static Dictionary<string, (bool SoloLectura, IReadOnlyList<string> Parametros)> ObtenerPropiedadesDeHerramientas()
    {
        var resultado = new Dictionary<string, (bool, IReadOnlyList<string>)>(StringComparer.Ordinal);
        foreach (var propiedad in Declaradas())
        {
            var atributo = LeerAtributo(propiedad);
            if (atributo is null)
            {
                continue;
            }

            var nombre = atributo.GetType().GetProperty("Name")!.GetValue(atributo) as string ?? string.Empty;
            var soloLectura = atributo.GetType().GetProperty("ReadOnly")?.GetValue(atributo) is true;
            var parametros = propiedad.GetParameters()
                .Select(parametro => parametro.Name ?? string.Empty)
                .ToArray();
            resultado[nombre] = (soloLectura, parametros);
        }

        return resultado;
    }

    private static object? LeerAtributo(MethodInfo metodo) =>
        metodo.GetCustomAttributes(inherit: true)
            .FirstOrDefault(atributo => atributo.GetType().Name == "McpServerToolAttribute");

    private static IEnumerable<MethodInfo> Declaradas() =>
        typeof(HerramientasPlannerGraph).GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .Concat(typeof(HerramientasEscrituraPlannerGraph).GetMethods(BindingFlags.Public | BindingFlags.Instance))
            .Where(metodo => metodo.GetCustomAttributes(inherit: true)
                .Any(atributo => atributo.GetType().Name == "McpServerToolAttribute"));

    private sealed class ManejadorGraphSimulado(Func<HttpRequestMessage, HttpResponseMessage> respuesta) : HttpMessageHandler
    {
        public List<HttpRequestMessage> Solicitudes { get; } = [];

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            Solicitudes.Add(request);
            return Task.FromResult(respuesta(request));
        }
    }
}
