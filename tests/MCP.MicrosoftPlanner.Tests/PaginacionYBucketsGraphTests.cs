using System.Net;
using System.Text;
using MCP.MicrosoftPlanner.Graph;
using Microsoft.Graph;
using Microsoft.Kiota.Abstractions.Authentication;

namespace MCP.MicrosoftPlanner.Tests;

public sealed class PaginacionYBucketsGraphTests
{
    [Fact]
    public async Task ListarBuckets_CuandoGraphDevuelvePaginaSiguiente_LeeTodasLasPaginas()
    {
        var manejador = new ManejadorGraphSimulado(c =>
        {
            if (c.RequestUri!.Query.Contains("skiptoken", StringComparison.Ordinal))
            {
                return Json("{\"value\":[{\"id\":\"b2\",\"name\":\"Segundo\",\"planId\":\"p1\"}]}");
            }

            return Json(
                "{\"value\":[{\"id\":\"b1\",\"name\":\"Primero\",\"planId\":\"p1\"}]," +
                "\"@odata.nextLink\":\"https://graph.microsoft.com/v1.0/planner/plans/p1/buckets?$skiptoken=2\"}");
        });

        var servicio = CrearServicio(manejador);

        var buckets = await servicio.ListarBucketsAsync("p1", CancellationToken.None);

        Assert.Equal(2, buckets.Count);
        Assert.Equal("Primero", buckets[0].Nombre);
        Assert.Equal("Segundo", buckets[1].Nombre);
        Assert.Equal(2, manejador.Solicitudes.Count);
    }

    [Fact]
    public async Task ListarTareas_CuandoGraphDevuelvePaginaSiguiente_LeeTodasLasPaginas()
    {
        var manejador = new ManejadorGraphSimulado(c =>
        {
            if (c.RequestUri!.Query.Contains("skiptoken", StringComparison.Ordinal))
            {
                return Json("{\"value\":[{\"id\":\"t2\",\"title\":\"Segunda\",\"planId\":\"p1\"}]}");
            }

            return Json(
                "{\"value\":[{\"id\":\"t1\",\"title\":\"Primera\",\"planId\":\"p1\"}]," +
                "\"@odata.nextLink\":\"https://graph.microsoft.com/v1.0/planner/plans/p1/tasks?$skiptoken=2\"}");
        });

        var servicio = CrearServicio(manejador);

        var tareas = await servicio.ListarTareasAsync("p1", CancellationToken.None);

        Assert.Equal(2, tareas.Count);
        Assert.Equal("Primera", tareas[0].Titulo);
        Assert.Equal("Segunda", tareas[1].Titulo);
        Assert.Equal(2, manejador.Solicitudes.Count);
    }

    [Fact]
    public async Task ListarBuckets_CuandoNoHayPaginaSiguiente_HaceUnaSolaSolicitud()
    {
        var manejador = new ManejadorGraphSimulado(_ =>
            Json("{\"value\":[{\"id\":\"b1\",\"name\":\"Primero\",\"planId\":\"p1\"}]}"));

        var servicio = CrearServicio(manejador);

        var buckets = await servicio.ListarBucketsAsync("p1", CancellationToken.None);

        Assert.Single(buckets);
        Assert.Single(manejador.Solicitudes);
    }

    [Fact]
    public void ObtenerFaltantes_CuandoExistenNombresQueSoloDifierenEnMayusculas_NoDuplicaLaPeticion()
    {
        var faltantes = Escritura.NombresBucketsPlanner.ObtenerFaltantes(
            new[] { "Trabajo", "trabajo" },
            new[] { "Trabajo", "Hogar" });

        Assert.Equal(new[] { "Hogar" }, faltantes);
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
        public List<Uri> Solicitudes { get; } = [];

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            Solicitudes.Add(request.RequestUri!);
            return Task.FromResult(respuesta(request));
        }
    }
}
