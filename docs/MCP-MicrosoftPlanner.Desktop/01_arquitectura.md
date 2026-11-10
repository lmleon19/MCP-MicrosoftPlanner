# MCP.MicrosoftPlanner.Desktop — Arquitectura

## Responsabilidad

Recoger los dos identificadores no secretos del registro de aplicación y persistirlos en la configuración del usuario. El modo gráfico no supervisa el servidor MCP.

## Entradas

| Origen | Contrato |
| --- | --- |
| Usuario | Tenant ID y Client ID escritos en la ventana. |
| Configuración del usuario | Archivo existente que se lee al abrir la ventana. |

## Salidas

| Destino | Contrato |
| --- | --- |
| Configuración del usuario | Archivo JSON con la sección `MicrosoftEntra`. |
| Interfaz | Mensaje de estado tras cada lectura o guardado. |

## Componentes internos

| Componente | Responsabilidad |
| --- | --- |
| App | Crea la ventana y termina al cerrarla. |
| MainWindow | Muestra los campos, valida la entrada y guarda la configuración. |
| ConfiguracionPlannerGraph | Ruta de configuración del usuario y guardado atómico. |

## Flujo principal

1. El usuario abre la aplicación; aparece la ventana con los valores guardados, si existen.
2. El usuario escribe el Tenant ID y el Client ID y selecciona guardar.
3. La aplicación valida que ambos campos tengan contenido y escribe el archivo de forma atómica.
4. Al cerrar la ventana, el configurador termina; la configuración permanece en el perfil del usuario.

## Relación con el servidor MCP

El ejecutable publicado tiene dos modos: sin argumentos abre el configurador, y con `--mcp` inicia el servidor por stdio sin cargar la interfaz. Cada inicio es un proceso independiente; el configurador no indica si el servidor MCP está conectado ni su estado de autenticación.

## Consideraciones de interfaz

Cerrar la ventana termina el proceso del configurador, sin detener el proceso MCP iniciado por el cliente.
