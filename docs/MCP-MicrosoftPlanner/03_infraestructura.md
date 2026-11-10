# MCP.MicrosoftPlanner — Infraestructura

## Recursos requeridos

| Recurso | Uso | Responsable | Comprobación |
| --- | --- | --- | --- |
| Equipo local | Ejecutar la consola MCP en Windows, macOS o Linux. | Usuario. | `dotnet --info` o el paquete autocontenido. |
| Registro Entra cliente público | Inicio de sesión delegado. | Administrador Entra. | Autenticación interactiva. |
| Microsoft Graph | Planner básico v1.0. | Microsoft. | Consulta de plan autorizada. |

## Dependencias externas

| Dependencia | Condición operacional |
| --- | --- |
| Microsoft Graph | Solo planes básicos; permisos `User.Read`, `Tasks.Read` y `Tasks.ReadWrite`. |

## Health checks y monitoreo

No expone HTTP ni health check. La disponibilidad se comprueba iniciando MCP y ejecutando una herramienta de lectura. Las mutaciones se verifican primero con una vista previa y requieren confirmación explícita.

## Procesos programados

No aplica.

## Diagnóstico inicial

| Síntoma | Comprobación inicial |
| --- | --- |
| No inicia | Verificar que `appsettings.json` exista junto al ejecutable y contenga las claves requeridas. |
| Acceso denegado | Verificar consentimiento de permisos delegados. |
| Error de redirección en el navegador | Verificar que `http://localhost` esté registrada como URI de redirección de plataforma. |
