using System.Text.Json;
using System.Text.Json.Nodes;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using MCP.MicrosoftPlanner.Configuracion;

namespace MCP.MicrosoftPlanner.Desktop;

public sealed partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        var rutaServidor = ConfiguracionClienteMcp.EncontrarServidor(Environment.ProcessPath);
        ServerPathText.Text = rutaServidor ?? string.Empty;
        CopyConfigButton.IsEnabled = rutaServidor is not null;
        if (rutaServidor is null)
        {
            ServerPathStatusText.Text = "No se pudo encontrar el ejecutable. Copia su ruta manualmente.";
        }

        try
        {
            var path = ConfiguracionPlannerGraph.RutaUsuario;
            if (File.Exists(path))
            {
                var root = JsonNode.Parse(File.ReadAllText(path));
                TenantInput.Text = root?["MicrosoftEntra"]?["TenantId"]?.GetValue<string>();
                ClientInput.Text = root?["MicrosoftEntra"]?["ClientId"]?.GetValue<string>();
                SoloLecturaInput.IsChecked = root?["MicrosoftEntra"]?["SoloLectura"]?.GetValue<bool>();
            }
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException or InvalidOperationException)
        {
            StatusText.Text = "No se pudo leer la configuración. Revisa el archivo de configuración antes de guardar.";
        }

    }

    private async void CopyOpenCodeConfig(object? sender, RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(ServerPathText.Text)) return;

        try
        {
            var clipboard = TopLevel.GetTopLevel(this)?.Clipboard;
            if (clipboard is null)
            {
                ServerPathStatusText.Text = "El portapapeles no está disponible. Copia la ruta mostrada manualmente.";
                return;
            }

            await clipboard.SetTextAsync(ConfiguracionClienteMcp.CrearConfiguracion(ServerPathText.Text));
            ServerPathStatusText.Text = "Configuración copiada. Añade el servidor planner a tu configuración de OpenCode; no reemplaces las demás entradas.";
        }
        catch (Exception ex) when (ex is InvalidOperationException or System.Runtime.InteropServices.ExternalException)
        {
            ServerPathStatusText.Text = "No se pudo copiar al portapapeles. Copia la ruta mostrada manualmente.";
        }
    }

    private void SaveSettings(object? sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(TenantInput.Text) || string.IsNullOrWhiteSpace(ClientInput.Text))
        {
            RestartHintText.IsVisible = false;
            StatusText.Text = "Se requieren el identificador del inquilino y el de la aplicación.";
            return;
        }

        try
        {
            ConfiguracionPlannerGraph.Guardar(
                ConfiguracionPlannerGraph.RutaUsuario,
                TenantInput.Text,
                ClientInput.Text,
                SoloLecturaInput.IsChecked == true);
            StatusText.Text = "Configuración guardada.";
            RestartHintText.IsVisible = true;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException or InvalidOperationException)
        {
            RestartHintText.IsVisible = false;
            StatusText.Text = "No se pudo guardar la configuración. Revisa los permisos del archivo y su formato.";
        }
    }
}
