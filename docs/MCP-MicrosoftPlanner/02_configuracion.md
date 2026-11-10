# MCP.MicrosoftPlanner — Configuración

## Fuentes permitidas

`src/MCP.MicrosoftPlanner/appsettings.json` contiene valores de ejemplo para desarrollo. El paquete de usuario no lo incluye: el configurador guarda los identificadores en el perfil del usuario. Se puede crear un `appsettings.json` opcional junto al ejecutable.

El configurador guarda las claves no secretas en `MCP/PlannerGraph/appsettings.json` dentro de la carpeta de datos de aplicación del usuario. Si este archivo existe, sus valores tienen prioridad sobre los del archivo base. Los cambios se aplican al reiniciar el proceso MCP. La ventana termina al cerrarla. El cliente MCP inicia el mismo ejecutable con `--mcp`, sin abrir la ventana. Consulte la [documentación del configurador](../MCP-MicrosoftPlanner.Desktop/02_configuracion.md) para el detalle de esa ruta.

## Parámetros

| Clave | Propósito | Obligatoria | Predeterminado | Sensible |
| --- | --- | ---: | --- | ---: |
| `MicrosoftEntra:TenantId` | Directorio Microsoft Entra. | Sí | No aplica | No |
| `MicrosoftEntra:ClientId` | Cliente público Microsoft Entra. | Sí | No aplica | No |
| `MicrosoftEntra:SoloLectura` | Limita la aplicación a lectura. | No | `false` | No |

El repositorio no incluye valores operativos: cada organización usa su propio registro de aplicación. Configure el tenant y el cliente en el archivo del usuario o mediante el configurador de escritorio.

## Modo de solo lectura

Con `MicrosoftEntra:SoloLectura` en `true` la aplicación:

- Solicita únicamente los permisos delegados `User.Read` y `Tasks.Read`.
- No registra las herramientas que modifican Planner, de modo que el catálogo MCP solo expone lecturas.

El modo se aplica al iniciar el proceso. Un valor que no sea `true` o `false` detiene el inicio indicando el nombre de la clave.

## Variación por ambiente

| Ambiente | Diferencias relevantes |
| --- | --- |
| Local | `appsettings.json` y caché MSAL por usuario. |

## Validación

La aplicación detiene el inicio si falta una clave; el mensaje muestra solo los nombres de claves.

## Datos sensibles

Los tokens se guardan mediante MSAL Extensions en el almacenamiento seguro del usuario. Nunca se muestran en diagnósticos, respuestas de error, código, pruebas ni documentación narrativa.

La aplicación solicita los permisos delegados `User.Read` y `Tasks.Read` para leer, y agrega `Tasks.ReadWrite` cuando el modo de solo lectura está desactivado. Las escrituras requieren además confirmación explícita de la herramienta.

Una operación de escritura solo se ejecuta cuando la herramienta recibe `confirmar: true`. Una petición explícita del usuario para crear o modificar recursos de Planner autoriza el lote completo, sin confirmaciones intermedias. Un plan no se selecciona por nombre si la búsqueda exacta devuelve cero o varias coincidencias.
