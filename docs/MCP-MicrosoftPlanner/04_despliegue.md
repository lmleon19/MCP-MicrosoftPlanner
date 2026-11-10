# MCP.MicrosoftPlanner — Despliegue

## Objetivo de despliegue

Publicar el ejecutable autocontenido o dependiente del runtime para uso local con OpenCode.

## Ficha de entrega a infraestructura

| Campo | Valor |
| --- | --- |
| Ambiente | Equipo local del usuario. |
| URL, hosting y servidor | No aplica; proceso local. |
| Responsable y ventana | Pendiente de definición por operación. |

## Artefacto de despliegue

| Elemento | Origen | Destino | Versión |
| --- | --- | --- | --- |
| Ejecutable único (configurador y MCP) | Release de GitHub o script de publicación | Equipo local | Inmutable por publicación. |

## Prerrequisitos

| Requisito | Detalle | Responsable | Estado |
| --- | --- | --- | --- |
| .NET 10 | Runtime compatible o publicación autocontenida. | Usuario. | Requerido si se usa el modo dependiente del runtime. |
| Aplicación Entra | Cliente público con permisos delegados de lectura y escritura aprobados. | Administrador Entra. | Requerido. |
| URI de redirección | `http://localhost` registrada como URI de redirección de plataforma en el registro de la aplicación. | Administrador Entra. | Requerido. |

## Configuración requerida

Consultar `02_configuracion.md`; nunca reproducir valores operativos.

## Procedimiento

1. Descargar el paquete de la versión publicada, o generarlo con `scripts/publicar-plataformas.ps1`.
2. Abrir el ejecutable para guardar la configuración, o crear opcionalmente un `appsettings.json` junto a él.
3. Registrar el servidor en el cliente MCP con la ruta absoluta del mismo ejecutable y el argumento `--mcp`.

### Registro en OpenCode

El servidor se registra con la ruta absoluta del ejecutable. El cliente lo inicia como un
proceso local, de modo que necesita la ubicación del archivo, no un nombre.

```jsonc
{
  "$schema": "https://opencode.ai/config.json",
  "mcp": {
    "servers": {
      "planner": {
        "type": "local",
        "command": ["/ruta/absoluta/al/ejecutable/MCP.MicrosoftPlanner", "--mcp"]
      }
    }
  }
}
```

En Windows, la ruta termina en `MCP.MicrosoftPlanner.exe`. En macOS y Linux el archivo no
lleva extensión y puede requerir `chmod +x` después de extraer el paquete. El servidor usa el
transporte stdio: el cliente lo inicia con `--mcp` y se comunica por `stdin/stdout`.

El comando equivalente evita editar el archivo:

```sh
opencode mcp add planner -- /ruta/absoluta/al/ejecutable/MCP.MicrosoftPlanner --mcp
```

Sin argumentos, el mismo ejecutable abre el configurador y escribe la configuración del usuario.
La ventana muestra su ruta absoluta y permite copiar el bloque `mcp` para agregarlo a OpenCode sin reemplazar otras entradas.

## Avisos de licencia en el paquete

Cada paquete publicado incluye `LICENSE`, `NOTICE` y `THIRD-PARTY-NOTICES.md` junto al único ejecutable. Las publicaciones son autocontenidas: redistribuyen el runtime de .NET y los paquetes de terceros dentro del binario, por lo que sus avisos deben acompañar a la distribución. Quien redistribuya los paquetes debe conservarlos.

## Comprobación posterior

| Comprobación | Resultado esperado |
| --- | --- |
| Herramienta de identidad | Respuesta MCP sin contaminación de stdout. |
| Listado de herramientas | Expone lectura y escritura con confirmación explícita. |

## Reversión

1. Deshabilitar el servidor MCP en la configuración de OpenCode o restaurar el artefacto anterior.
