using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Configuration;

namespace MCP.MicrosoftPlanner.Configuracion;

/// <summary>Representa la configuración no secreta requerida para Microsoft Entra.</summary>
public sealed record ConfiguracionPlannerGraph(string TenantId, string ClientId, bool SoloLectura = false)
{
    public const string Seccion = "MicrosoftEntra";
    public const string TenantIdClave = $"{Seccion}:TenantId";
    public const string ClientIdClave = $"{Seccion}:ClientId";
    public const string SoloLecturaClave = $"{Seccion}:SoloLectura";

    /// <summary>
    /// Permisos delegados que la aplicación solicita. En modo de solo lectura basta <c>Tasks.Read</c>;
    /// con escritura se requiere además <c>Tasks.ReadWrite</c>.
    /// </summary>
    public IReadOnlyList<string> Permisos =>
        SoloLectura ? ["User.Read", "Tasks.Read"] : ["User.Read", "Tasks.Read", "Tasks.ReadWrite"];

    public static string RutaUsuario => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "MCP", "PlannerGraph", "appsettings.json");

    public static ConfiguracionPlannerGraph CargarArchivos(string rutaBase, string rutaUsuario)
    {
        var configuracion = new ConfigurationBuilder()
            .AddJsonFile(rutaBase, optional: true, reloadOnChange: false)
            .AddJsonFile(rutaUsuario, optional: true, reloadOnChange: false)
            .Build();
        return Cargar(configuracion);
    }

    public static void Guardar(string ruta, string tenantId, string clientId, bool soloLectura = false)
    {
        if (string.IsNullOrWhiteSpace(tenantId) || string.IsNullOrWhiteSpace(clientId))
        {
            throw new ArgumentException("Both tenant and client identifiers are required.");
        }

        var root = File.Exists(ruta) ? JsonNode.Parse(File.ReadAllText(ruta)) as JsonObject : new JsonObject();
        if (root is null) throw new JsonException();
        var section = root[Seccion] as JsonObject;
        if (root[Seccion] is not null && section is null) throw new JsonException();
        if (section is null)
        {
            section = new JsonObject();
            root[Seccion] = section;
        }

        section["TenantId"] = tenantId.Trim();
        section["ClientId"] = clientId.Trim();
        section["SoloLectura"] = soloLectura;
        var directorio = Path.GetDirectoryName(Path.GetFullPath(ruta))!;
        Directory.CreateDirectory(directorio);
        var temporal = Path.Combine(directorio, $".planner-{Guid.NewGuid():N}.tmp");
        try
        {
            File.WriteAllText(temporal, root.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));
            File.Move(temporal, ruta, overwrite: true);
        }
        finally
        {
            if (File.Exists(temporal)) File.Delete(temporal);
        }
    }

    /// <summary>Carga y valida la configuración requerida desde appsettings.</summary>
    public static ConfiguracionPlannerGraph Cargar(IConfiguration configuracion)
    {
        ArgumentNullException.ThrowIfNull(configuracion);
        var tenantId = configuracion[TenantIdClave];
        var clientId = configuracion[ClientIdClave];

        if (string.IsNullOrWhiteSpace(tenantId) || string.IsNullOrWhiteSpace(clientId))
        {
            throw new InvalidOperationException(
                $"Se requieren los valores de configuración {TenantIdClave} y {ClientIdClave}.");
        }

        return new ConfiguracionPlannerGraph(tenantId.Trim(), clientId.Trim(), LeerSoloLectura(configuracion));
    }

    private static bool LeerSoloLectura(IConfiguration configuracion)
    {
        var valor = configuracion[SoloLecturaClave];
        if (string.IsNullOrWhiteSpace(valor))
        {
            return false;
        }

        if (!bool.TryParse(valor, out var soloLectura))
        {
            throw new InvalidOperationException(
                $"El valor de configuración {SoloLecturaClave} debe ser verdadero o falso.");
        }

        return soloLectura;
    }
}
