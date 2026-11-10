# Seguridad

## Versiones soportadas

| Versión | Soportada |
| --- | --- |
| 1.x | Sí |

## Reportar una vulnerabilidad

No abra una issue pública ni un pull request para reportar una vulnerabilidad. Eso podría exponer a otras personas antes de que exista una corrección.

Use el reporte de seguridad privada de GitHub: en la pestaña **Security** del repositorio, elija **Report a vulnerability**. Si no está disponible, contacte al mantenedor por un canal privado.

Incluya:

- Descripción de la vulnerabilidad y su impacto.
- Pasos para reproducir.
- Versión del ejecutable, del runtime y del sistema operativo.
- Cualquier prueba o captura que ayude a confirmarla.

## Qué esperar

- Acuse de recibo dentro de los siete días.
- Evaluación inicial dentro de los treinta días.
- Información sobre la corrección o la razón para no corregir.

Las vulnerabilidades se corrigen con prioridad y la corrección se publica con una entrada en `CHANGELOG.md`.

## Alcance esperado

Esta aplicación maneja tokens delegados de Microsoft Entra y datos de Planner. Reporte, entre otros:

- Exposición de tokens, credenciales o identificadores en logs, respuestas o errores.
- Evasión de la confirmación explícita o del ETag en operaciones de escritura.
- Permisos superiores a los declarados.
- Inyección de contenido en las respuestas de las herramientas.
