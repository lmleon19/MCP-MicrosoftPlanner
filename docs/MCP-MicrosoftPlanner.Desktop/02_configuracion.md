# MCP.MicrosoftPlanner.Desktop — Configuración

## Fuentes permitidas

El configurador lee y escribe un único archivo: `MCP/PlannerGraph/appsettings.json` dentro de la carpeta de datos de aplicación del usuario. La ruta se resuelve desde `ConfiguracionPlannerGraph.RutaUsuario`:

| Sistema | Ubicación |
| --- | --- |
| Windows | `%APPDATA%\MCP\PlannerGraph\appsettings.json` |
| macOS y Linux | `$XDG_CONFIG_HOME/MCP/PlannerGraph/appsettings.json` o `~/.config/MCP/PlannerGraph/appsettings.json` |

El paquete no incluye un `appsettings.json` junto al ejecutable; si el usuario lo crea, el configurador no lo modifica y conserva menor prioridad.

## Parámetros

| Clave | Propósito | Obligatoria | Predeterminado | Sensible |
| --- | --- | ---: | --- | ---: |
| `MicrosoftEntra:TenantId` | Directorio de Microsoft Entra. | Sí | Vacío | No |
| `MicrosoftEntra:ClientId` | Cliente público de Microsoft Entra. | Sí | Vacío | No |

## Variación por ambiente

| Ambiente | Diferencias relevantes |
| --- | --- |
| Local | Archivo único en el perfil del usuario; no hay configuración por ambiente. |

## Validación

La ventana exige que ambos campos tengan contenido antes de guardar. La lectura inicial tolera un archivo ausente. Si el archivo existe pero no se puede leer o no es JSON válido, la aplicación muestra un aviso y deja los campos vacíos para que el usuario los escriba de nuevo. El guardado escribe un archivo temporal y lo mueve sobre el destino, de modo que una operación fallida no deja el archivo a medias.

## Datos sensibles

Los dos identificadores no son secretos: el Client ID identifica el registro de aplicación y puede mostrarse. No se registran tokens, contraseñas ni otros datos sensibles. El archivo de configuración del usuario no se incluye en el repositorio ni en los paquetes publicados.
