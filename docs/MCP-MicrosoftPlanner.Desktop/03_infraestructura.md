# MCP.MicrosoftPlanner.Desktop — Infraestructura

## Recursos requeridos

| Recurso | Uso | Responsable | Comprobación |
| --- | --- | --- | --- |
| Entorno gráfico | Ventana de configuración. | Usuario | La ventana abre y termina al cerrarla. |
| Carpeta de datos de aplicación | Archivo de configuración del usuario. | Usuario | El archivo se crea tras guardar. |
| Registro Entra cliente público | Valores que se configuran. | Administrador Entra | La aplicación valida el inicio de sesión del servidor MCP. |

## Dependencias externas

| Dependencia | Condición operacional |
| --- | --- |
| Microsoft Entra | No se contacta desde el configurador. Solo se validan los identificadores escritos. |

## Health checks y monitoreo

No expone HTTP ni health check. La disponibilidad se comprueba abriendo la ventana y guardando los valores. El modo MCP por stdio puede funcionar sin abrir la interfaz gráfica.

## Procesos programados

No aplica. El proceso del configurador termina al cerrar la ventana.

## Diagnóstico inicial

| Síntoma | Comprobación inicial |
| --- | --- |
| No abre la ventana | Verificar que haya una sesión gráfica disponible. |
| No guarda | Verificar permisos de escritura en la carpeta de datos de aplicación. |
| Los valores no aparecen al abrir | Verificar que exista el archivo de configuración del usuario y sea JSON válido. |
