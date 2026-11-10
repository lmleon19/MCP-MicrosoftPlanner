using Microsoft.Identity.Client;

namespace MCP.MicrosoftPlanner.Autenticacion;

/// <summary>Define el flujo interactivo de MSAL para clientes públicos de escritorio.</summary>
public static class OpcionesInteractiveMicrosoft
{
    /// <summary>
    /// URI de redirección de loopback. MSAL reserva un puerto libre en tiempo de ejecución, por lo que
    /// no debe fijarse un puerto: registrar <c>http://localhost</c> como URI de redirección de plataforma.
    /// </summary>
    public const string RedirectUri = "http://localhost";

    /// <summary>Prepara las opciones del navegador del sistema para el flujo interactivo.</summary>
    public static SystemWebViewOptions CrearOpciones() => new()
    {
        HtmlMessageSuccess = """
            <html><head><meta charset="utf-8"><title>Autenticación completada</title></head>
            <body style="font-family: system-ui, sans-serif; text-align: center; padding: 3rem;">
            <h1>Autenticación completada</h1><p>Puede cerrar esta ventana.</p></body></html>
            """,
        HtmlMessageError = """
            <html><head><meta charset="utf-8"><title>Error de autenticación</title></head>
            <body style="font-family: system-ui, sans-serif; text-align: center; padding: 3rem;">
            <h1>No se pudo completar la autenticación</h1><p>Puede cerrar esta ventana.</p></body></html>
            """,
    };
}
