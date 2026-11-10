using Microsoft.Kiota.Abstractions;
using Microsoft.Kiota.Abstractions.Authentication;

namespace MCP.MicrosoftPlanner.Autenticacion;

/// <summary>Agrega el token delegado de MSAL a solicitudes Microsoft Graph.</summary>
public sealed class ProveedorAutenticacionGraph(ProveedorTokenMsal proveedorToken) : IAuthenticationProvider
{
    /// <inheritdoc />
    public async Task AuthenticateRequestAsync(
        RequestInformation request,
        Dictionary<string, object>? additionalAuthenticationContext = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var token = await proveedorToken.ObtenerTokenAsync(cancellationToken).ConfigureAwait(false);
        request.Headers.TryAdd("Authorization", $"Bearer {token}");
    }
}
