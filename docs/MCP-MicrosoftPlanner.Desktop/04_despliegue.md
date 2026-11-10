# MCP.MicrosoftPlanner.Desktop — Despliegue

## Objetivo de despliegue

Publicar un ejecutable único por plataforma que abre el configurador sin argumentos y el servidor MCP con `--mcp`.

## Ficha de entrega a infraestructura

| Campo | Valor |
| --- | --- |
| Ambiente | Equipo local del usuario. |
| URL, hosting y servidor | No aplica; proceso local. |
| Responsable y ventana | Pendiente de definición por operación. |

## Artefacto de despliegue

| Elemento | Origen | Destino | Versión |
| --- | --- | --- | --- |
| Ejecutable único | Script de publicación | Equipo local | Inmutable por publicación. |

Los paquetes incluyen un solo ejecutable en la raíz.

Cada paquete incluye `LICENSE`, `NOTICE` y `THIRD-PARTY-NOTICES.md` junto al ejecutable. Las publicaciones son autocontenidas y redistribuyen el runtime de .NET y los paquetes de terceros dentro del binario, por lo que sus avisos deben acompañar a la distribución.

## Prerrequisitos

| Requisito | Detalle | Responsable | Estado |
| --- | --- | --- | --- |
| Entorno de escritorio | Windows, macOS o Linux con interfaz gráfica. | Usuario. | Requerido para configurar mediante la ventana. |
| Registro Entra cliente público | Identificadores que se escriben en la ventana. | Administrador Entra. | Requerido. |

## Configuración requerida

Consultar `02_configuracion.md`; nunca reproducir valores operativos.

## Procedimiento

1. Publicar la aplicación con `scripts/publicar-plataformas.ps1` o descargar el paquete publicado.
2. Ejecutar `MCP.MicrosoftPlanner` sin argumentos para abrir el configurador.
3. Escribir el Tenant ID y el Client ID y guardar.
4. Registrar el mismo ejecutable con `--mcp` en el cliente MCP y reiniciarlo para que lea los valores.

## Comprobación posterior

| Comprobación | Resultado esperado |
| --- | --- |
| Ventana de configuración | Abre y muestra los valores guardados previamente, o campos vacíos si no existen. |
| Guardado | Mensaje de confirmación y archivo creado en el perfil del usuario. |
| Cierre de ventana | Termina el configurador sin detener el servidor iniciado por el cliente MCP. |

## Reversión

1. Eliminar el archivo `MCP/PlannerGraph/appsettings.json` de la carpeta de datos de aplicación del usuario.
2. Cerrar la ventana del configurador.
