using System.Net;
using System.Text;
using MCP.MicrosoftPlanner.Graph;
using Microsoft.Graph;
using Microsoft.Kiota.Abstractions.Authentication;

namespace MCP.MicrosoftPlanner.Tests;

public sealed class PlanesYEliminacionGraphTests
{
    [Fact]
    public async Task ListarPlanes_CuandoGraphDevuelvePaginaSiguiente_LeeTodasLasPaginas()
    {
        var manejador = new ManejadorGraphSimulado(c =>
        {
            if (c.RequestUri!.Query.Contains("skiptoken", StringComparison.Ordinal))
            {
                return Json("{\"value\":[{\"id\":\"p2\",\"title\":\"Segundo\"}]}");
            }

            return Json(
                "{\"value\":[{\"id\":\"p1\",\"title\":\"Primero\"}]," +
                "\"@odata.nextLink\":\"https://graph.microsoft.com/v1.0/me/planner/plans?$skiptoken=2\"}");
        });

        var servicio = CrearServicio(manejador);

        var planes = await servicio.ListarPlanesAsync(CancellationToken.None);

        Assert.Equal(2, planes.Count);
        Assert.Equal("Primero", planes[0].Titulo);
        Assert.Equal("Segundo", planes[1].Titulo);
        Assert.Equal(2, manejador.Solicitudes.Count);
    }

    [Fact]
    public async Task ListarPlanes_CuandoNoHayPaginaSiguiente_HaceUnaSolaSolicitud()
    {
        var manejador = new ManejadorGraphSimulado(_ =>
            Json("{\"value\":[{\"id\":\"p1\",\"title\":\"Primero\"}]}"));

        var servicio = CrearServicio(manejador);

        var planes = await servicio.ListarPlanesAsync(CancellationToken.None);

        Assert.Single(planes);
        Assert.Single(manejador.Solicitudes);
    }

    [Fact]
    public async Task EliminarTarea_CuandoElEtagCoincide_EnviaDeleteConIfMatch()
    {
        var manejador = new ManejadorGraphSimulado(_ => new HttpResponseMessage(HttpStatusCode.NoContent));

        var servicio = CrearServicio(manejador);

        await servicio.EliminarTareaAsync("t1", "W/\"etag-1\"", CancellationToken.None);

        var solicitud = Assert.Single(manejador.Solicitudes);
        Assert.Equal(HttpMethod.Delete, solicitud.Method);
        Assert.Equal("W/\"etag-1\"", solicitud.Headers.IfMatch.ToString());
    }

    [Fact]
    public async Task EliminarTarea_CuandoFaltaElEtag_RechazaAntesDeLlamarAGraph()
    {
        var manejador = new ManejadorGraphSimulado(_ => new HttpResponseMessage(HttpStatusCode.NoContent));

        var servicio = CrearServicio(manejador);

        await Assert.ThrowsAsync<ArgumentException>(
            () => servicio.EliminarTareaAsync("t1", "  ", CancellationToken.None));
        Assert.Empty(manejador.Solicitudes);
    }

    private static ServicioPlannerGraph CrearServicio(ManejadorGraphSimulado manejador) =>
        new(new GraphServiceClient(
            new HttpClient(manejador) { BaseAddress = new Uri("https://graph.microsoft.com/v1.0") },
            new AnonymousAuthenticationProvider(),
            "https://graph.microsoft.com/v1.0"));

    private static HttpResponseMessage Json(string contenido) => new(HttpStatusCode.OK)
    {
        Content = new StringContent(contenido, Encoding.UTF8, "application/json"),
    };

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
