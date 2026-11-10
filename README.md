# MCP Microsoft Planner

**Versión del proyecto: 1.0.0** · Aún sin versión publicada para descarga.

**Acceda a Planner básico desde su cliente MCP sin pasar por Work IQ.** Este servidor local usa Microsoft Graph, autenticación delegada de Microsoft Entra y transporte stdio. El mismo ejecutable abre un configurador para guardar los identificadores de cada organización.

[Empezar](#inicio-rápido) · [Herramientas](#herramientas-disponibles) · [Solucionar problemas](#solucionar-problemas) · [Contribuir](CONTRIBUTING.md)

Windows · macOS · Linux | x64 · ARM64 | [Licencia MIT](LICENSE)

## Por qué existe

Microsoft ofrece **[Work IQ MCP](https://learn.microsoft.com/en-us/microsoft-365/copilot/extensibility/work-iq/mcp/overview)** para conectar agentes con datos de Microsoft 365, incluido Planner. Sin embargo, [Work IQ API se factura por uso mediante Copilot Credits](https://learn.microsoft.com/en-us/microsoft-365/copilot/extensibility/work-iq/): una licencia individual de Microsoft 365 Copilot **no es obligatoria** para acceder a la API, pero tampoco elimina necesariamente el cobro por uso en agentes personalizados o de terceros. No se trata de una tarifa fija «por token»; el consumo depende del trabajo realizado.

Si lo que se necesita es **consultar y modificar planes básicos de Planner**, esa capa de inteligencia y su modelo de cobro pueden resultar innecesarios. Este proyecto ofrece herramientas MCP específicas que llaman directamente a la [API de Planner en Microsoft Graph v1.0](https://learn.microsoft.com/en-us/graph/api/resources/planner-overview): **no invoca Work IQ ni consume Copilot Credits por Work IQ**. Sigue requiriendo acceso autorizado a Planner, los permisos delegados correspondientes y las licencias de Microsoft 365 que correspondan a la organización. Los costos del cliente de IA que use el MCP son independientes.

La diferencia de alcance es importante: **[la API de Planner en Graph solo expone planes básicos, no planes premium](https://learn.microsoft.com/en-us/graph/api/resources/planner-overview)**. Este servidor no sustituye las capacidades de búsqueda semántica, razonamiento y contexto empresarial de Work IQ.

> **Estado de distribución:** el repositorio puede generar seis paquetes autocontenidos; aún no se ha publicado una versión descargable. Para probarlo ahora, compile desde el código fuente o genere un paquete localmente.

## Qué incluye

- **Lectura:** identidad, planes, buckets, tareas y detalles; búsqueda por título.
- **Escritura controlada:** creación y actualización de tareas y sus detalles; creación de buckets y categorías faltantes; eliminación de tareas con confirmación y ETag.
- **Solo lectura opcional:** menos permisos de Microsoft Graph y ninguna herramienta de escritura.
- **Un ejecutable por plataforma:** abre el configurador al hacer doble clic y actúa como servidor MCP por stdio al iniciarlo con `--mcp`. La ventana no mantiene la conexión MCP.

No incluye Work IQ ni alojamiento en Azure. Cada organización aporta su propio registro de aplicación en Microsoft Entra; no se distribuyen secretos de aplicación.

## Inicio rápido

1. [Obtenga el programa](#1-obtener-el-programa): descargue y extraiga el paquete de su plataforma cuando haya una versión publicada, o compile desde el código.
2. [Prepare Microsoft Entra](#2-preparar-microsoft-entra): registre un cliente público y anote el Tenant ID y el Client ID.
3. [Guarde los identificadores](#3-guardar-la-configuración): abra el ejecutable del paquete y guarde la configuración.
4. [Conecte su cliente MCP](#4-conectar-un-cliente-mcp): apunte el cliente al mismo ejecutable con el argumento `--mcp`.
5. Solicite a su cliente que invoque `obtener_identidad` o `listar_planes`. La autenticación interactiva se abrirá en el navegador cuando haga falta.

### 1. Obtener el programa

Los paquetes incluyen .NET 10; **no hace falta instalar el runtime** para usarlos. Elija el archivo correspondiente a su sistema y arquitectura, y extraiga **todo** su contenido en una carpeta permanente:

| Sistema | Archivo generado |
| --- | --- |
| Windows x64 / ARM64 | `MCP.MicrosoftPlanner-win-x64.zip` / `MCP.MicrosoftPlanner-win-arm64.zip` |
| Linux x64 / ARM64 | `MCP.MicrosoftPlanner-linux-x64.tar.gz` / `MCP.MicrosoftPlanner-linux-arm64.tar.gz` |
| macOS Intel / Apple Silicon | `MCP.MicrosoftPlanner-osx-x64.tar.gz` / `MCP.MicrosoftPlanner-osx-arm64.tar.gz` |

En la carpeta extraída encontrará un solo ejecutable de la aplicación, `MCP.MicrosoftPlanner.exe` (Windows) o `MCP.MicrosoftPlanner` (macOS/Linux), y los avisos de licencia. **No es un instalador:** no registra el MCP en los clientes ni instala un servicio del sistema.

En macOS/Linux, si el sistema no permite ejecutar el binario después de extraer el archivo, otorgue permiso con `chmod +x MCP.MicrosoftPlanner` desde la carpeta extraída. Para abrir el configurador se requiere un entorno gráfico.

**Desde el código fuente** (requiere SDK .NET 10):

```sh
git clone https://github.com/lmleon19/MCP-MicrosoftPlanner.git
cd MCP-MicrosoftPlanner
dotnet build MCP.MicrosoftPlanner.slnx -c Release
```

Para generar los seis archivos localmente (requiere PowerShell y SDK .NET 10):

```sh
pwsh scripts/publicar-plataformas.ps1
```

Quedan en `dist/`. Generar paquetes en Windows no verifica su ejecución en macOS o Linux.

### 2. Preparar Microsoft Entra

Cada organización debe:

1. Registrar una aplicación **cliente público** en Microsoft Entra.
2. Agregar la URI de redirección `http://localhost` sin puerto fijo.
3. Conceder permisos **delegados** de Microsoft Graph: `User.Read` y `Tasks.Read`; agregar `Tasks.ReadWrite` solo si se usarán herramientas de escritura.
4. Entregar al usuario el **Tenant ID** y el **Client ID**. El Client ID identifica la aplicación; no es una contraseña.

El primer acceso puede pedir inicio de sesión y consentimiento en el navegador. La autenticación sin un token previo en caché todavía requiere verificación manual en un entorno real.

### 3. Guardar la configuración

**Con el paquete extraído:** abra `MCP.MicrosoftPlanner` (o `.exe`), ingrese Tenant ID y Client ID, elija si desea **Solo lectura** y pulse **Guardar configuración**. El configurador guarda los datos por usuario y termina al cerrar la ventana. Si cambia los valores, reinicie la conexión al servicio MCP desde el cliente donde lo use (OpenCode, Claude u otro) para que el servidor los vuelva a leer.

También puede crear opcionalmente un `appsettings.json` junto al ejecutable con la sección `MicrosoftEntra`:

```json
{
  "MicrosoftEntra": {
    "TenantId": "ID-DEL-TENANT",
    "ClientId": "ID-DE-LA-APLICACION",
    "SoloLectura": true
  }
}
```

La configuración de usuario escrita por la ventana tiene prioridad sobre ese archivo. Con `SoloLectura: true`, el servidor solo solicita `User.Read` y `Tasks.Read`, y no registra herramientas de escritura. No incluya identificadores propios de su organización en el repositorio.

### 4. Conectar un cliente MCP

Todos los ejemplos siguientes usan el **mismo ejecutable** del paquete en modo stdio. Reemplace `C:/Users/tu-usuario/Planner/MCP.MicrosoftPlanner.exe` por su ruta absoluta; en macOS/Linux use `/ruta/absoluta/MCP.MicrosoftPlanner`. En JSON, las barras `/` también funcionan en Windows y evitan escapar `\`. Conserve siempre `--mcp`: sin él se abre el configurador gráfico. `http://localhost` es únicamente la redirección de inicio de sesión de Microsoft Entra, **no** una dirección del servidor MCP. Integre la entrada `planner` en su configuración existente, sin borrar otros servidores.

#### OpenCode

El configurador muestra la **ruta absoluta del mismo ejecutable** y ofrece **Copiar configuración para OpenCode** con `--mcp`. Pegue el bloque `mcp` en su `opencode.json`, integrándolo con las entradas que ya tenga; el botón no modifica OpenCode automáticamente.

Ejemplo de configuración local (reemplace la ruta por la que muestra el configurador):

```jsonc
{
  "$schema": "https://opencode.ai/config.json",
  "mcp": {
    "servers": {
      "planner": {
        "type": "local",
        "command": ["C:/Users/tu-usuario/Planner/MCP.MicrosoftPlanner.exe", "--mcp"]
      }
    }
  }
}
```

En macOS/Linux la ruta termina en `/MCP.MicrosoftPlanner` sin `.exe`. Es una **ruta local**, no una URL: OpenCode inicia ese proceso con `--mcp` y se comunica por stdin/stdout. Verifique la conexión desde `/mcps` o `opencode mcp list`.

Si trabaja desde el repositorio clonado, `opencode.json` ya registra `dotnet` y la ruta relativa a `src/MCP.MicrosoftPlanner/bin/Release/net10.0/MCP.MicrosoftPlanner.dll`; requiere compilar y tener .NET 10. [Configuración oficial de OpenCode](https://opencode.ai/docs/mcp-servers).

#### Claude Code

Agregue el servidor con la CLI (el alcance predeterminado es local al proyecto):

```sh
claude mcp add --transport stdio planner -- "C:/Users/tu-usuario/Planner/MCP.MicrosoftPlanner.exe" --mcp
claude mcp list
```

Para compartirlo en un proyecto, agregue `--scope project` al primer comando (antes de `planner`); Claude Code lo registra en `.mcp.json`. También puede inspeccionarlo con `/mcp`. [Guía oficial de Claude Code](https://code.claude.com/docs/en/mcp).

#### Claude Desktop

Abra **Settings → Developer → Edit Config** y agregue la entrada en `claude_desktop_config.json` (Windows: `%APPDATA%\Claude\`; macOS: `~/Library/Application Support/Claude/`; Linux: `~/.config/Claude/`):

```json
{
  "mcpServers": {
    "planner": {
      "command": "C:/Users/tu-usuario/Planner/MCP.MicrosoftPlanner.exe",
      "args": ["--mcp"]
    }
  }
}
```

Cierre Claude Desktop completamente y vuelva a abrirlo; revise **Connectors** para ver las herramientas. [Guía oficial de MCP para Claude Desktop](https://modelcontextprotocol.io/docs/2026-07-28/develop/connect-local-servers).

#### Codex CLI, extensión IDE y aplicación ChatGPT de escritorio

En `~/.codex/config.toml` (o `.codex/config.toml` de un proyecto confiable), agregue:

```toml
[mcp_servers.planner]
command = "C:/Users/tu-usuario/Planner/MCP.MicrosoftPlanner.exe"
args = ["--mcp"]
```

Compruebe con `codex mcp list` o `/mcp` en Codex. Los clientes locales de Codex comparten esta configuración; **ChatGPT web no inicia servidores stdio locales**. [Guía oficial de Codex](https://developers.openai.com/codex/mcp).

#### VS Code con GitHub Copilot

Agregue esta entrada a `.vscode/mcp.json` del proyecto, o a la configuración de usuario de servidores MCP de VS Code:

```json
{
  "servers": {
    "planner": {
      "type": "stdio",
      "command": "C:/Users/tu-usuario/Planner/MCP.MicrosoftPlanner.exe",
      "args": ["--mcp"]
    }
  }
}
```

Inicie el servidor desde la vista de MCP de VS Code y revise su estado antes de usar las herramientas en el chat. [Referencia oficial de VS Code](https://code.visualstudio.com/docs/agents/reference/mcp-configuration).

#### Cursor

Agregue la entrada a `~/.cursor/mcp.json` (global) o `.cursor/mcp.json` (proyecto):

```json
{
  "mcpServers": {
    "planner": {
      "type": "stdio",
      "command": "C:/Users/tu-usuario/Planner/MCP.MicrosoftPlanner.exe",
      "args": ["--mcp"]
    }
  }
}
```

Revise el servidor en **Customize**; Cursor solicita aprobación antes de ejecutar herramientas de forma predeterminada. [Guía oficial de Cursor](https://cursor.com/docs/mcp).

#### Gemini CLI

Agregue la entrada a `~/.gemini/settings.json` (usuario) o `.gemini/settings.json` (proyecto):

```json
{
  "mcpServers": {
    "planner": {
      "command": "C:/Users/tu-usuario/Planner/MCP.MicrosoftPlanner.exe",
      "args": ["--mcp"]
    }
  }
}
```

Compruebe con `/mcp` o `gemini mcp list`; Gemini puede mostrar el servidor como desconectado si el directorio actual no es confiable. [Guía oficial de Gemini CLI](https://github.com/google-gemini/gemini-cli/blob/main/docs/tools/mcp-server.md).

#### Otros clientes

Si su cliente admite **MCP local por stdio**, indique como comando la ruta absoluta del ejecutable y como único argumento `--mcp`. Consulte la documentación de ese cliente para conocer el nombre de la clave y dónde se guarda; algunos clientes solo admiten MCP remoto o restringen la ejecución local. No use la ruta del configurador, una URL de Work IQ ni la redirección OAuth. Tras cambiar la configuración, reinicie o reconecte el servidor MCP **en ese cliente**.

**Ejemplo de uso:** una vez conectado, solicite «Muéstrame los planes a los que tengo acceso» para usar `listar_planes`. Para editar, primero lea el recurso y revise la previsualización; las operaciones de escritura requieren confirmación explícita.

## Herramientas disponibles

El servidor expone 18 herramientas. OpenCode las publica como `<servidor>_<herramienta>`, por lo que con el servidor `planner` quedan disponibles como `planner_listar_tareas`, `planner_crear_tarea` y así sucesivas.

### Lectura

| Herramienta | Qué hace | Parámetros |
| --- | --- | --- |
| `obtener_identidad` | Identidad del usuario autenticado en Microsoft Graph. | — |
| `listar_planes` | Lista todos los planes accesibles por el usuario. | — |
| `buscar_plan_por_nombre` | Busca planes por coincidencia exacta de título y devuelve todas las coincidencias. | `nombrePlan` |
| `obtener_plan` | Obtiene un plan por identificador. | `planId` |
| `obtener_detalles_plan` | Etiquetas definidas en el plan y su ETag. | `planId` |
| `listar_buckets` | Lista las columnas de un plan. | `planId` |
| `listar_tareas` | Lista las tareas de un plan con su estado, fechas y categorías. | `planId` |
| `buscar_tareas` | Busca texto o un identificador externo en los títulos de las tareas. | `planId`, `consulta` |
| `obtener_tarea` | Obtiene una tarea por identificador. | `tareaId` |
| `obtener_detalle_tarea` | Descripción, checklist y ETag de una tarea. | `tareaId` |
| `previsualizar_buckets` | Indica qué columnas faltan, sin crear nada. | `planId`, `nombres` |
| `previsualizar_categorias` | Indica qué etiquetas faltan, sin crear nada. | `planId`, `nombres` |

### Escritura

Todas exigen `confirmar: true` y, cuando modifican un recurso existente, el `eTag` obtenido en una lectura previa.

| Herramienta | Qué hace | Parámetros |
| --- | --- | --- |
| `crear_tarea` | Crea una tarea en una columna. | `planId`, `bucketId`, `titulo`, `confirmar` |
| `actualizar_tarea` | Modifica título, columna, progreso, fechas, categorías o responsables. | `tareaId`, `eTag`, `actualizacion`, `confirmar` |
| `actualizar_detalle_tarea` | Modifica la descripción y el checklist de una tarea. | `tareaId`, `eTag`, `descripcion`, `checklist`, `confirmar` |
| `eliminar_tarea` | Elimina una tarea de forma permanente. | `tareaId`, `eTag`, `confirmar` |
| `asegurar_buckets` | Crea solo las columnas que faltan. | `planId`, `nombres`, `confirmar` |
| `asegurar_categorias` | Crea solo las etiquetas que faltan. | `planId`, `nombres`, `confirmar` |

`eliminar_tarea` es la única operación irreversible: Microsoft Graph no permite recuperar una tarea eliminada. Exige el `eTag` vigente, por lo que solo se puede borrar una tarea que se leyó antes.

Con `SoloLectura` en `true` no se expone ninguna herramienta de escritura, y la aplicación solicita únicamente `User.Read` y `Tasks.Read`.

## Solucionar problemas

| Situación | Qué revisar |
| --- | --- |
| El configurador no muestra la ruta del servidor | Extraiga el paquete completo: el servidor debe estar junto a la carpeta `desktop`. Si ejecuta el desktop desde el proyecto compilado, no se garantiza esa estructura. |
| OpenCode no conecta | Confirme que `command` apunta al ejecutable del servidor, no al desktop, y consulte el estado en `/mcps`. En macOS/Linux, compruebe el permiso de ejecución. |
| Faltan Tenant ID o Client ID | Abra el configurador y guarde ambos valores. No basta con registrar el ejecutable en OpenCode. |
| Los cambios no se reflejan | Desconecte y vuelva a conectar el servicio MCP en el cliente que lo usa. El servidor lee la configuración al arrancar; cerrar el configurador no reinicia el servidor. |
| No aparecen herramientas de escritura | Revise **Solo lectura** y los permisos delegados concedidos a la aplicación en Microsoft Entra. |

## Documentación

| Guía | Contenido |
| --- | --- |
| [Configuración](docs/MCP-MicrosoftPlanner/02_configuracion.md) | Valores de Microsoft Entra y opciones. |
| [Despliegue](docs/MCP-MicrosoftPlanner/04_despliegue.md) | Publicación y conexión como servidor local. |
| [Documentación del proyecto](docs/README.md) | Índice de arquitectura y operación. |

## Desarrollo

```sh
dotnet restore MCP.MicrosoftPlanner.slnx
dotnet build MCP.MicrosoftPlanner.slnx -c Release
dotnet test MCP.MicrosoftPlanner.slnx -c Release
dotnet format MCP.MicrosoftPlanner.slnx --verify-no-changes --no-restore
```

## Contribuir

Lea [CONTRIBUTING.md](CONTRIBUTING.md). El [CHANGELOG.md](CHANGELOG.md) registra los cambios y el [código de conducta](CODE_OF_CONDUCT.md) fija las expectativas de la comunidad. Para reportar una vulnerabilidad, consulte [SECURITY.md](SECURITY.md).

## Licencia y atribución

Este proyecto es obra de **Luis Miguel León Valenzuela** y se distribuye bajo la licencia MIT. Consulte [LICENSE](LICENSE) y [NOTICE](NOTICE).

Si utiliza, modifica o adapta este proyecto, conservar esta atribución en su distribución:

```text
MCP Microsoft Planner
Copyright (c) 2026 Luis Miguel León Valenzuela
https://github.com/lmleon19/MCP-MicrosoftPlanner
```

Proyecto original: https://github.com/lmleon19/MCP-MicrosoftPlanner

Las dependencias de terceros que se redistribuyen en los paquetes autocontenidos conservan sus propias licencias, enumeradas en [THIRD-PARTY-NOTICES.md](THIRD-PARTY-NOTICES.md). Entre ellas, el SDK de C# para Model Context Protocol se distribuye bajo Apache-2.0 y se consume sin modificaciones.

Consulte [la documentación operativa](docs/README.md) y el [historial de cambios](CHANGELOG.md).
