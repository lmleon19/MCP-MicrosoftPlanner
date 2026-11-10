# MCP.MicrosoftPlanner — Arquitectura

## Responsabilidad

Traducir herramientas MCP genéricas de Planner básico en llamadas delegadas a Microsoft Graph.

## Entradas

| Origen | Contrato |
| --- | --- |
| OpenCode | MCP por stdin. |
| Entorno local | Tenant ID y Client ID. |

## Salidas

| Destino | Contrato |
| --- | --- |
| OpenCode | MCP por stdout. |
| Microsoft Graph | HTTPS v1.0 con token delegado. |
| Consola local | Diagnósticos sanitizados por stderr. |

## Componentes internos

| Componente | Responsabilidad |
| --- | --- |
| Herramientas | Contratos MCP de lectura y escritura confirmada. |
| Servicio Graph | Consultas, mutaciones y DTOs mínimos. |
| MSAL | Token interactivo y caché segura. |
| Configurador de escritorio | Ventana para editar Tenant ID y Client ID; se cierra al terminar. |

## Paginación

Los listados de planes, buckets y tareas recorren las páginas sucesivas que devuelve Graph mediante `@odata.nextLink`. Sin este recorrido, un plan con más elementos que el tamaño de página devolvería resultados incompletos y `asegurar_buckets` podría crear un bucket con un nombre ya existente en una página posterior.

## Escrituras controladas

Las operaciones de creación exigen `confirmar: true`. Las actualizaciones de tarea o detalle exigen además el ETag obtenido en una consulta previa y lo envían en `If-Match`. El servidor no renombra recursos Planner; puede retirar un ítem de checklist identificado cuando se concilia una tarea.

La eliminación de una tarea es la única operación irreversible. La herramienta `eliminar_tarea` exige `confirmar: true` y el ETag vigente, que Graph usa como control de concurrencia en `If-Match`: una tarea modificada después de la lectura produce `412` en lugar de borrarse. No hay forma de recuperar una tarea eliminada.

Las herramientas `previsualizar_buckets` y `asegurar_buckets` permiten calcular y crear solo los buckets faltantes. `obtener_detalles_plan`, `previsualizar_categorias` y `asegurar_categorias` permiten leer, calcular y crear solo las etiquetas faltantes del plan (`category1..category25`). Las demás escrituras genéricas son `crear_tarea`, `actualizar_tarea`, `actualizar_detalle_tarea` y `eliminar_tarea`. La lectura de detalles devuelve los ítems existentes; la actualización crea o modifica los ítems con título y retira exclusivamente los ítems identificados cuyo título sea nulo. La actualización de tarea puede marcar o desmarcar categorías explícitas, agregar asignaciones sin retirar las existentes y establecer `startDateTime`; siempre fusiona el estado actual antes del PATCH. En `actualizar_detalle_tarea`, una `descripcion` nula vacía la descripción en Graph: reenvía la descripción vigente (la de una consulta previa) en cada actualización de checklist.

`buscar_plan_por_nombre` busca entre los planes accesibles por coincidencia exacta y devuelve todas las coincidencias; el consumidor solo puede continuar automáticamente si obtiene una única.

## Flujo principal

1. OpenCode inicia el ejecutable local.
2. Una herramienta solicita un token silencioso o abre el navegador local.
3. El servicio consulta Graph y devuelve datos mínimos por MCP.

## Autenticación

MSAL administra el flujo interactivo: reserva un puerto de loopback libre, abre el navegador del sistema y recibe la respuesta de redirección. La aplicación no implementa un receptor propio de loopback, porque competiría con el de MSAL por el mismo puerto. La URI de redirección registrada en Microsoft Entra es `http://localhost`, sin puerto fijo.
