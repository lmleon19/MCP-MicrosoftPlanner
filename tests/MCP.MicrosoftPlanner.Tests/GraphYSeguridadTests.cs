using MCP.MicrosoftPlanner.Diagnostico;
using MCP.MicrosoftPlanner.Escritura;
using MCP.MicrosoftPlanner.Graph;
using Microsoft.Graph.Models;
using Microsoft.Kiota.Abstractions.Serialization;

namespace MCP.MicrosoftPlanner.Tests;

public sealed class GraphYSeguridadTests
{
    [Fact]
    public void DetalleTareaPlanner_CuandoIncluyeChecklist_ConservaLosElementos()
    {
        var checklist = new[] { new ElementoChecklistPlanner("f8061e6f-6a8d-4a87-9793-14c7983a7ca3", "PL-01: Paso", false) };

        var detalle = new DetalleTareaPlanner("Descripción", "etag", checklist);

        Assert.Single(detalle.Checklist!);
        Assert.Equal("PL-01: Paso", detalle.Checklist![0].Titulo);
    }

    [Fact]
    public void ElementoChecklistPlanner_CuandoTituloEsNulo_RepresentaEliminacion()
    {
        var elemento = new ElementoChecklistPlanner("f8061e6f-6a8d-4a87-9793-14c7983a7ca3", null, false);

        Assert.Null(elemento.Titulo);
    }

    [Fact]
    public void CrearChecklist_CuandoIncluyeVariosElementos_DeclaraElTipoODataEnCadaElemento()
    {
        var checklist = ChecklistTareasPlanner.Crear(
        [
            new ElementoChecklistPlanner("f8061e6f-6a8d-4a87-9793-14c7983a7ca3", "PL-01: Paso", false),
            new ElementoChecklistPlanner("50a9f7d7-5f44-4ad0-b9ca-3840b1228c75", "PL-02: Otro paso", true),
        ]);

        Assert.Equal(2, checklist.AdditionalData.Count);
        Assert.All(checklist.AdditionalData.Values, valor =>
        {
            var item = Assert.IsType<PlannerChecklistItem>(valor);
            Assert.Equal("#microsoft.graph.plannerChecklistItem", item.OdataType);
        });
    }

    [Fact]
    public void MapearChecklist_CuandoGraphDevuelveElementosSinTipo_LosConserva()
    {
        var checklist = new PlannerChecklistItems
        {
            AdditionalData = new Dictionary<string, object>
            {
                ["f8061e6f-6a8d-4a87-9793-14c7983a7ca3"] = new UntypedObject(
                    new Dictionary<string, UntypedNode>
                    {
                        ["title"] = new UntypedString("PL-01: Paso"),
                        ["isChecked"] = new UntypedBoolean(true),
                    }),
            },
        };

        var elementos = ChecklistTareasPlanner.Mapear(checklist);

        var elemento = Assert.Single(elementos);
        Assert.Equal("PL-01: Paso", elemento.Titulo);
        Assert.True(elemento.Completado);
    }

    [Theory]
    [InlineData(401, "no_autenticado")]
    [InlineData(403, "acceso_denegado")]
    [InlineData(404, "no_encontrado")]
    [InlineData(409, "conflicto")]
    [InlineData(412, "etag_invalido")]
    [InlineData(429, "limite_superado")]
    public void Traducir_CuandoGraphDevuelveEstadoConocido_DevuelveCodigoSeguro(int estado, string codigo)
    {
        var error = TraductorErroresGraph.Traducir(estado);

        Assert.Equal(codigo, error.Codigo);
        Assert.Equal(estado, error.EstadoHttp);
    }

    [Fact]
    public void TareasPlan_CuandoIdentificadorContieneCaracteresReservados_EscapaRutaGraph()
    {
        var solicitud = SolicitudesPlanner.TareasPlan("plan/a b");

        Assert.Equal("https://graph.microsoft.com/v1.0/planner/plans/plan%2Fa%20b/tasks", solicitud.AbsoluteUri);
    }

    [Fact]
    public void Validar_CuandoNoHayConfirmacion_RechazaMutacion()
    {
        Assert.Throws<InvalidOperationException>(() => GuardiaEscrituraPlanner.Validar(false, "etag"));
    }

    [Fact]
    public void Validar_CuandoETagEstaVacio_RechazaMutacion()
    {
        Assert.Throws<InvalidOperationException>(() => GuardiaEscrituraPlanner.Validar(true, " "));
    }

    [Fact]
    public void ValidarCreacion_CuandoNoHayConfirmacion_RechazaMutacion()
    {
        Assert.Throws<InvalidOperationException>(() => GuardiaEscrituraPlanner.ValidarCreacion(false));
    }

    [Fact]
    public void ObtenerFaltantes_CuandoNombresDifierenSoloEnEspaciosOMayusculas_DevuelveSoloLosNuevos()
    {
        var faltantes = NombresBucketsPlanner.ObtenerFaltantes(
            [" Propuestas ", "Aprobadas"],
            ["propuestas", " Pendientes de aprobación ", "PENDIENTES DE APROBACIÓN", " "]);

        Assert.Equal(["Pendientes de aprobación"], faltantes);
    }

    [Fact]
    public void AsignarClavesLibres_CuandoHayHuecos_ReutilizaElPrimerHuecoSinDuplicarNombres()
    {
        var asignadas = NombresEtiquetasPlanner.AsignarClavesLibres(
            new Dictionary<string, string>
            {
                ["category1"] = "Propuesta",
                ["category3"] = "Aprobada",
            },
            [" propuesta ", "Implementada", "Cancelada"]);

        Assert.Equal("category2", asignadas["Implementada"]);
        Assert.Equal("category4", asignadas["Cancelada"]);
        Assert.DoesNotContain("Propuesta", asignadas.Keys, StringComparer.OrdinalIgnoreCase);
    }

    [Fact]
    public void AsignarClavesLibres_CuandoNoHayHuecos_RechazaNuevaEtiqueta()
    {
        var existentes = Enumerable.Range(1, NombresEtiquetasPlanner.MaximoEtiquetasPorPlan)
            .ToDictionary(indice => $"category{indice}", indice => $"Etiqueta {indice}");

        Assert.Throws<InvalidOperationException>(() =>
            NombresEtiquetasPlanner.AsignarClavesLibres(existentes, ["Nueva etiqueta"]));
    }

    [Theory]
    [InlineData("category1", true)]
    [InlineData("category25", true)]
    [InlineData("category0", false)]
    [InlineData("category26", false)]
    [InlineData("otra", false)]
    public void EsClaveCategoria_CuandoLaClaveEstaDentroDelRango_ValidaCorrectamente(string clave, bool esperado)
    {
        Assert.Equal(esperado, NombresEtiquetasPlanner.EsClaveCategoria(clave));
    }

    [Fact]
    public void FusionarCategorias_CuandoSeAgregaUnaEtiqueta_ConservaLasExistentes()
    {
        var fusionadas = FusionTareaPlanner.FusionarCategorias(
            [new KeyValuePair<string, bool>("category1", true), new KeyValuePair<string, bool>("category2", false)],
            ["category3"]);

        Assert.True(fusionadas["category1"]);
        Assert.False(fusionadas["category2"]);
        Assert.True(fusionadas["category3"]);
    }

    [Fact]
    public void FusionarCategorias_CuandoSeDesmarcaUnaEtiqueta_ConservaCategoriasAjenas()
    {
        var fusionadas = FusionTareaPlanner.FusionarCategorias(
            [new KeyValuePair<string, bool>("category1", true), new KeyValuePair<string, bool>("category7", true)],
            ["category4"],
            ["category1"]);

        Assert.False(fusionadas["category1"]);
        Assert.True(fusionadas["category4"]);
        Assert.True(fusionadas["category7"]);
    }

    [Fact]
    public void ObtenerAsignadosFaltantes_CuandoHayAsignadosPrevios_ConservaYDevuelveSoloLosNuevos()
    {
        var faltantes = FusionTareaPlanner.ObtenerAsignadosFaltantes(
            ["usuario-existente"],
            ["USUARIO-EXISTENTE", "usuario-nuevo", " usuario-nuevo "]);

        Assert.Equal(["usuario-nuevo"], faltantes);
    }

    [Theory]
    [InlineData(" MCP-MicrosoftPlanner ", "MCP-MicrosoftPlanner", true)]
    [InlineData("MCP-MicrosoftPlanner - copia", "MCP-MicrosoftPlanner", false)]
    [InlineData(null, "MCP-MicrosoftPlanner", false)]
    public void CoincideExactamente_CuandoElTituloNoEsExacto_NoSeleccionaElPlan(
        string? titulo,
        string nombrePlan,
        bool esperado)
    {
        Assert.Equal(esperado, NombresPlanesPlanner.CoincideExactamente(titulo, nombrePlan));
    }

    [Fact]
    public void Sanitizar_CuandoMensajeContieneAutorizacionOToken_LosRedacta()
    {
        var resultado = RegistroSeguro.Sanitizar("Authorization: Bearer token-secreto");

        Assert.DoesNotContain("token-secreto", resultado, StringComparison.Ordinal);
        Assert.Contains("[REDACTADO]", resultado, StringComparison.Ordinal);
    }

    [Fact]
    public void EscribirError_EnviaDiagnosticoSoloAStderr()
    {
        var salidaOriginal = Console.Out;
        var errorOriginal = Console.Error;
        using var salida = new StringWriter();
        using var error = new StringWriter();

        try
        {
            Console.SetOut(salida);
            Console.SetError(error);
            RegistroSeguro.EscribirError("fallo Bearer token-secreto");
        }
        finally
        {
            Console.SetOut(salidaOriginal);
            Console.SetError(errorOriginal);
        }

        Assert.Equal(string.Empty, salida.ToString());
        Assert.DoesNotContain("token-secreto", error.ToString(), StringComparison.Ordinal);
    }
}
