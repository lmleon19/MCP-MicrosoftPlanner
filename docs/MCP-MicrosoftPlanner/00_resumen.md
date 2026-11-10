# MCP.MicrosoftPlanner — Resumen

## Descripción

Consola .NET 10 que expone operaciones de Microsoft Planner básico como herramientas MCP locales para OpenCode. Es genérica y no conoce planes, repositorios ni archivos concretos.

## Ficha del componente

| Campo | Valor |
| --- | --- |
| Propósito | Consultar y actualizar Planner básico mediante Microsoft Graph v1.0. |
| Tipo | Consola MCP local. |
| Ubicación | `src/MCP.MicrosoftPlanner/`. |

## Incluye

Autenticación delegada, caché MSAL segura local y herramientas genéricas de lectura y escritura confirmada.

## No incluye

Work IQ, secretos de cliente ni hosting. La eliminación de tareas sí está disponible y exige confirmación explícita y el ETag vigente.

## Dependencias

| Dependencia | Uso |
| --- | --- |
| Microsoft Entra | Autenticación delegada de usuario. |
| Microsoft Graph v1.0 | Planner básico. |
| OpenCode | Cliente MCP local. |
