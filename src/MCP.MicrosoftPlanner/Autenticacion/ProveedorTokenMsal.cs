using MCP.MicrosoftPlanner.Configuracion;
using Microsoft.Identity.Client;
using Microsoft.Identity.Client.Extensions.Msal;

namespace MCP.MicrosoftPlanner.Autenticacion;

/// <summary>Obtiene tokens delegados mediante MSAL y una caché segura local por usuario.</summary>
public sealed class ProveedorTokenMsal
{
    private readonly ConfiguracionPlannerGraph _configuracion;
    private readonly IRepositorioTokenMsal _repositorio;
    private readonly Lazy<Task<IPublicClientApplication>> _aplicacion;

    public ProveedorTokenMsal(ConfiguracionPlannerGraph configuracion)
        : this(configuracion, null)
    {
    }

    internal ProveedorTokenMsal(ConfiguracionPlannerGraph configuracion, IRepositorioTokenMsal? repositorio)
    {
        _configuracion = configuracion;
        _aplicacion = new Lazy<Task<IPublicClientApplication>>(CrearAplicacionAsync);
        _repositorio = repositorio ?? new RepositorioTokenMsal(_aplicacion, configuracion.Permisos);
    }

    /// <summary>Obtiene un token silencioso o inicia autenticación interactiva en el navegador local.</summary>
    public async Task<string> ObtenerTokenAsync(CancellationToken cancellationToken)
    {
        try
        {
            return await _repositorio.ObtenerTokenSilenciosoAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (MsalUiRequiredException)
        {
            // La interacción explícita de usuario es el mecanismo previsto para renovar la sesión.
        }

        return await _repositorio.ObtenerTokenInteractivoAsync(cancellationToken).ConfigureAwait(false);
    }

    private async Task<IPublicClientApplication> CrearAplicacionAsync()
    {
        var aplicacion = PublicClientApplicationBuilder
            .Create(_configuracion.ClientId)
            .WithAuthority(AzureCloudInstance.AzurePublic, _configuracion.TenantId)
            .WithRedirectUri(OpcionesInteractiveMicrosoft.RedirectUri)
            .Build();

        var directorioCache = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "MCP",
            "PlannerGraph");
        Directory.CreateDirectory(directorioCache);

        var propiedadesCache = new StorageCreationPropertiesBuilder("msal.cache", directorioCache).Build();
        var cacheHelper = await MsalCacheHelper.CreateAsync(propiedadesCache).ConfigureAwait(false);
        cacheHelper.RegisterCache(aplicacion.UserTokenCache);
        return aplicacion;
    }

    private sealed class RepositorioTokenMsal(
        Lazy<Task<IPublicClientApplication>> aplicacion,
        IReadOnlyList<string> permisos) : IRepositorioTokenMsal
    {
        public async Task<string> ObtenerTokenSilenciosoAsync(CancellationToken cancellationToken)
        {
            var cliente = await aplicacion.Value.ConfigureAwait(false);
            var cuenta = (await cliente.GetAccountsAsync().ConfigureAwait(false)).FirstOrDefault();
            if (cuenta is null)
            {
                throw new MsalUiRequiredException("no hay cuenta en la caché local", Guid.NewGuid().ToString());
            }

            var resultado = await cliente
                .AcquireTokenSilent(permisos, cuenta)
                .ExecuteAsync(cancellationToken)
                .ConfigureAwait(false);
            return resultado.AccessToken;
        }

        public async Task<string> ObtenerTokenInteractivoAsync(CancellationToken cancellationToken)
        {
            var cliente = await aplicacion.Value.ConfigureAwait(false);
            var resultado = await cliente
                .AcquireTokenInteractive(permisos)
                .WithSystemWebViewOptions(OpcionesInteractiveMicrosoft.CrearOpciones())
                .ExecuteAsync(cancellationToken)
                .ConfigureAwait(false);
            return resultado.AccessToken;
        }
    }
}
