using MCP.MicrosoftPlanner.Configuracion;
using Microsoft.Extensions.Configuration;

namespace MCP.MicrosoftPlanner.Tests;

public sealed class ConfiguracionPlannerGraphTests
{
    [Fact]
    public void Guardar_CuandoFaltaUnIdentificador_NoModificaElArchivo()
    {
        var directorio = Path.Combine(Path.GetTempPath(), $"planner-config-{Guid.NewGuid():N}");
        Directory.CreateDirectory(directorio);
        try
        {
            var path = Path.Combine(directorio, "user.json");
            File.WriteAllText(path, "{\"MicrosoftEntra\":{\"TenantId\":\"old\",\"ClientId\":\"old\"}}");
            var antes = System.Security.Cryptography.SHA256.HashData(File.ReadAllBytes(path));
            Assert.Throws<ArgumentException>(() => ConfiguracionPlannerGraph.Guardar(path, " ", "client"));
            Assert.Equal(antes, System.Security.Cryptography.SHA256.HashData(File.ReadAllBytes(path)));
        }
        finally { Directory.Delete(directorio, recursive: true); }
    }

    [Fact]
    public void GuardarYCargar_CuandoHayArchivoBase_UsaValoresGuardadosYConservaOtrosDatos()
    {
        var directorio = Path.Combine(Path.GetTempPath(), $"planner-config-{Guid.NewGuid():N}");
        Directory.CreateDirectory(directorio);
        try
        {
            var rutaBase = Path.Combine(directorio, "base.json");
            var rutaUsuario = Path.Combine(directorio, "user.json");
            File.WriteAllText(rutaBase, "{\"MicrosoftEntra\":{\"TenantId\":\"base\",\"ClientId\":\"base\"}}");
            File.WriteAllText(rutaUsuario, "{\"extra\":42}");
            ConfiguracionPlannerGraph.Guardar(rutaUsuario, " tenant ", " client ");
            var resultado = ConfiguracionPlannerGraph.CargarArchivos(rutaBase, rutaUsuario);
            Assert.Equal("tenant", resultado.TenantId);
            Assert.Equal("client", resultado.ClientId);
            Assert.Equal(42, System.Text.Json.Nodes.JsonNode.Parse(File.ReadAllText(rutaUsuario))!["extra"]!.GetValue<int>());
        }
        finally { Directory.Delete(directorio, recursive: true); }
    }

    [Fact]
    public void Guardar_CuandoElArchivoExiste_ConservaOtrasPropiedades()
    {
        var directorio = Path.Combine(Path.GetTempPath(), $"planner-config-{Guid.NewGuid():N}");
        Directory.CreateDirectory(directorio);
        try
        {
            var path = Path.Combine(directorio, "user.json");
            File.WriteAllText(path, "{\"Other\":\"keep\",\"MicrosoftEntra\":{\"Extra\":\"keep\"}}");
            ConfiguracionPlannerGraph.Guardar(path, "tenant", "client");
            var json = System.Text.Json.Nodes.JsonNode.Parse(File.ReadAllText(path))!;
            Assert.Equal("keep", json["Other"]!.GetValue<string>());
            Assert.Equal("keep", json["MicrosoftEntra"]!["Extra"]!.GetValue<string>());
            Assert.Equal("tenant", json["MicrosoftEntra"]!["TenantId"]!.GetValue<string>());
            Assert.Equal("client", json["MicrosoftEntra"]!["ClientId"]!.GetValue<string>());
        }
        finally { Directory.Delete(directorio, recursive: true); }
    }

    [Fact]
    public void Guardar_CuandoElArchivoEsInvalido_NoLoModifica()
    {
        var directorio = Path.Combine(Path.GetTempPath(), $"planner-config-{Guid.NewGuid():N}");
        Directory.CreateDirectory(directorio);
        try
        {
            var path = Path.Combine(directorio, "user.json");
            File.WriteAllText(path, "{invalid");
            Assert.ThrowsAny<System.Text.Json.JsonException>(() => ConfiguracionPlannerGraph.Guardar(path, "tenant", "client"));
            Assert.Equal("{invalid", File.ReadAllText(path));
        }
        finally { Directory.Delete(directorio, recursive: true); }
    }

    [Fact]
    public void CargarArchivos_CuandoExisteConfiguracionDeUsuario_TienePrioridadSobreElArchivoBase()
    {
        var directorio = Path.Combine(Path.GetTempPath(), $"planner-config-{Guid.NewGuid():N}");
        Directory.CreateDirectory(directorio);
        try
        {
            var basePath = Path.Combine(directorio, "base.json");
            var userPath = Path.Combine(directorio, "user.json");
            File.WriteAllText(basePath, "{\"MicrosoftEntra\":{\"TenantId\":\"base-tenant\",\"ClientId\":\"base-client\"}}");
            File.WriteAllText(userPath, "{\"MicrosoftEntra\":{\"TenantId\":\"user-tenant\",\"ClientId\":\"user-client\"}}");

            var configuracion = ConfiguracionPlannerGraph.CargarArchivos(basePath, userPath);

            Assert.Equal("user-tenant", configuracion.TenantId);
            Assert.Equal("user-client", configuracion.ClientId);
        }
        finally
        {
            Directory.Delete(directorio, recursive: true);
        }
    }

    [Fact]
    public void CargarArchivos_CuandoNoHayArchivoDeUsuario_ConservaConfiguracionBase()
    {
        var directorio = Path.Combine(Path.GetTempPath(), $"planner-config-{Guid.NewGuid():N}");
        Directory.CreateDirectory(directorio);
        try
        {
            var basePath = Path.Combine(directorio, "base.json");
            File.WriteAllText(basePath, "{\"MicrosoftEntra\":{\"TenantId\":\"base-tenant\",\"ClientId\":\"base-client\"}}");

            var configuracion = ConfiguracionPlannerGraph.CargarArchivos(basePath, Path.Combine(directorio, "missing.json"));

            Assert.Equal("base-tenant", configuracion.TenantId);
            Assert.Equal("base-client", configuracion.ClientId);
        }
        finally
        {
            Directory.Delete(directorio, recursive: true);
        }
    }

    [Fact]
    public void CargarArchivos_SinArchivoBase_UsaConfiguracionDeUsuario()
    {
        var directorio = Path.Combine(Path.GetTempPath(), $"planner-config-{Guid.NewGuid():N}");
        Directory.CreateDirectory(directorio);
        try
        {
            var rutaUsuario = Path.Combine(directorio, "user.json");
            ConfiguracionPlannerGraph.Guardar(rutaUsuario, "tenant", "client", soloLectura: true);

            var configuracion = ConfiguracionPlannerGraph.CargarArchivos(Path.Combine(directorio, "missing.json"), rutaUsuario);

            Assert.Equal("tenant", configuracion.TenantId);
            Assert.Equal("client", configuracion.ClientId);
            Assert.True(configuracion.SoloLectura);
        }
        finally { Directory.Delete(directorio, recursive: true); }
    }

    [Fact]
    public void Cargar_CuandoElModoEsSoloLectura_SolicitaSoloPermisosDeLectura()
    {
        var configuracionOrigen = CrearConfiguracion("tenant", "client", soloLectura: "true");

        var configuracion = ConfiguracionPlannerGraph.Cargar(configuracionOrigen);

        Assert.True(configuracion.SoloLectura);
        Assert.Equal(["User.Read", "Tasks.Read"], configuracion.Permisos);
        Assert.DoesNotContain("Tasks.ReadWrite", configuracion.Permisos);
    }

    [Fact]
    public void Cargar_CuandoElModoNoEsSoloLectura_SolicitaPermisosDeLecturaYEscritura()
    {
        var configuracionOrigen = CrearConfiguracion("tenant", "client");

        var configuracion = ConfiguracionPlannerGraph.Cargar(configuracionOrigen);

        Assert.False(configuracion.SoloLectura);
        Assert.Contains("Tasks.Read", configuracion.Permisos);
        Assert.Contains("Tasks.ReadWrite", configuracion.Permisos);
    }

    [Fact]
    public void Cargar_CuandoElModoSoloLecturaNoEsBooleano_LanzaErrorSinExponerValores()
    {
        var configuracionOrigen = CrearConfiguracion("tenant", "client", soloLectura: "quizá");

        var excepcion = Assert.Throws<InvalidOperationException>(() => ConfiguracionPlannerGraph.Cargar(configuracionOrigen));

        Assert.Contains(ConfiguracionPlannerGraph.SoloLecturaClave, excepcion.Message, StringComparison.Ordinal);
        Assert.DoesNotContain("tenant", excepcion.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Guardar_CuandoSeActivaSoloLectura_PersisteLaOpcion()
    {
        var directorio = Path.Combine(Path.GetTempPath(), $"planner-config-{Guid.NewGuid():N}");
        Directory.CreateDirectory(directorio);
        try
        {
            var path = Path.Combine(directorio, "user.json");

            ConfiguracionPlannerGraph.Guardar(path, "tenant", "client", soloLectura: true);

            var configuracion = ConfiguracionPlannerGraph.CargarArchivos(path, path);
            Assert.True(configuracion.SoloLectura);
        }
        finally
        {
            Directory.Delete(directorio, recursive: true);
        }
    }

    [Fact]
    public void Cargar_CuandoExistenValoresEnAppsettings_DevuelveConfiguracionRecortada()
    {
        var configuracionOrigen = CrearConfiguracion(" tenant ", " client ");

        var configuracion = ConfiguracionPlannerGraph.Cargar(configuracionOrigen);

        Assert.Equal("tenant", configuracion.TenantId);
        Assert.Equal("client", configuracion.ClientId);
    }

    [Fact]
    public void Cargar_CuandoFaltaValor_LanzaErrorSinExponerValores()
    {
        var configuracionOrigen = CrearConfiguracion(null, null);

        var excepcion = Assert.Throws<InvalidOperationException>(() => ConfiguracionPlannerGraph.Cargar(configuracionOrigen));

        Assert.Contains(ConfiguracionPlannerGraph.TenantIdClave, excepcion.Message, StringComparison.Ordinal);
        Assert.DoesNotContain("token", excepcion.Message, StringComparison.OrdinalIgnoreCase);
    }

    private static IConfiguration CrearConfiguracion(string? tenantId, string? clientId, string? soloLectura = null) =>
        new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                [ConfiguracionPlannerGraph.TenantIdClave] = tenantId,
                [ConfiguracionPlannerGraph.ClientIdClave] = clientId,
                [ConfiguracionPlannerGraph.SoloLecturaClave] = soloLectura,
            })
            .Build();
}
