# Changelog

Todas las novedades de este proyecto se documentan en este archivo.

El formato sigue [Keep a Changelog](https://keepachangelog.com/es/1.1.0/) y el proyecto aplica [Versionado Semántico](https://semver.org/lang/es/).

## [Sin publicar]

### Añadido

- Configurador de escritorio con bandeja del sistema para editar el Tenant ID y el Client ID.
- Publicación autocontenida para Windows, macOS y Linux en arquitecturas x64 y arm64.
- Configuración por usuario con prioridad sobre el archivo junto al ejecutable.

### Corregido

- Los listados de buckets y de tareas recorren todas las páginas que devuelve Microsoft Graph.
- El aseguramiento de buckets tolera nombres existentes que solo difieren en mayúsculas.
- La redirección de autenticación la administra MSAL, sin un receptor local que compita por el puerto.

### Cambiado

- El proyecto se renombró a MCP Microsoft Planner y se preparó para distribución pública.

## [1.0.0]

### Añadido

- Servidor MCP local de Planner básico con Microsoft Graph v1.0 y autenticación delegada.
- Herramientas de lectura de identidad, planes, buckets, tareas y detalles.
- Herramientas de escritura con confirmación explícita y ETag para buckets, tareas y detalles.
- Búsqueda de plan por nombre exacto y búsqueda de tareas por título.
