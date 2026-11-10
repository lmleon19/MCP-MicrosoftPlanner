namespace MCP.MicrosoftPlanner.Graph;

/// <summary>Traduce estados HTTP conocidos a errores seguros para el protocolo MCP.</summary>
public static class TraductorErroresGraph
{
    /// <summary>Convierte un estado HTTP en un error sin detalles de Graph.</summary>
    public static ErrorHerramienta Traducir(int estadoHttp) => estadoHttp switch
    {
        401 => new ErrorHerramienta("no_autenticado", "La sesión de Microsoft Graph no está autenticada.", estadoHttp),
        403 => new ErrorHerramienta("acceso_denegado", "No tiene permisos para acceder al recurso solicitado.", estadoHttp),
        404 => new ErrorHerramienta("no_encontrado", "No se encontró el recurso solicitado.", estadoHttp),
        409 => new ErrorHerramienta("conflicto", "El recurso cambió de forma concurrente.", estadoHttp),
        412 => new ErrorHerramienta("etag_invalido", "La versión del recurso no coincide con el ETag proporcionado.", estadoHttp),
        429 => new ErrorHerramienta("limite_superado", "Microsoft Graph limitó temporalmente la solicitud.", estadoHttp),
        _ => new ErrorHerramienta("error_graph", "No fue posible completar la solicitud a Microsoft Graph.", estadoHttp),
    };
}
