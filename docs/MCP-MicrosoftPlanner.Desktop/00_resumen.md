# MCP.MicrosoftPlanner.Desktop — Resumen

## Descripción

Modo gráfico del ejecutable único que permite escribir el Tenant ID y el Client ID del registro de aplicación de Microsoft Entra sin editar archivos a mano. Termina al cerrar la ventana y deja la configuración en el perfil del usuario, donde el modo MCP la lee con prioridad sobre un archivo opcional junto al ejecutable. No administra planes, tareas ni permisos de Planner.

## Ficha del componente

| Campo | Valor |
| --- | --- |
| Propósito | Editar los identificadores de la aplicación de Microsoft Entra usados por el servidor MCP. |
| Tipo | Aplicación de escritorio Avalonia. |
| Ubicación | `src/MCP.MicrosoftPlanner.Desktop/`. |

## Incluye

Ventana con los dos identificadores, lectura de la configuración existente y guardado atómico en el perfil del usuario.

## No incluye

La ventana no mantiene la conexión MCP, autentica en Microsoft ni gestiona permisos, planes o tareas. El mismo ejecutable inicia el servidor en otro proceso al recibir `--mcp`.

## Dependencias

| Dependencia | Uso |
| --- | --- |
| Avalonia | Interfaz de escritorio multiplataforma. |
| MCP.MicrosoftPlanner | Reutiliza la ruta de configuración y las reglas de guardado. |
| Microsoft Entra | Registro de aplicación cuyos identificadores se configuran. |
