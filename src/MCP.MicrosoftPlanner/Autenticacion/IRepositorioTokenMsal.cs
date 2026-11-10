namespace MCP.MicrosoftPlanner.Autenticacion;

/// <summary>Obtiene tokens delegados de Microsoft Entra para las operaciones delegadas.</summary>
public interface IRepositorioTokenMsal
{
    /// <summary>Obtiene un token desde la caché local sin interacción del usuario.</summary>
    Task<string> ObtenerTokenSilenciosoAsync(CancellationToken cancellationToken);

    /// <summary>Obtiene un token iniciando la autenticación interactiva en el navegador.</summary>
    Task<string> ObtenerTokenInteractivoAsync(CancellationToken cancellationToken);
}
