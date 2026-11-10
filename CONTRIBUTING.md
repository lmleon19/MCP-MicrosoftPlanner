# Contributing

Gracias por querer mejorar MCP Microsoft Planner. Toda contribución es bienvenida: correcciones, documentación, traducciones y pruebas.

## Antes de empezar

- **Discuta los cambios grandes primero.** Abrir una issue describe el problema y la propuesta antes de escribir código. Evita trabajo duplicado y permite acordar el alcance.
- **Revise las issues abiertas.** Es posible que el problema ya esté reportado o solucionado.
- **Respete el alcance.** El servidor expone un conjunto genérico de herramientas de Planner. No se agregan funciones específicas de una organización.

## Cómo contribuir

1. Cree una rama desde `main` con un nombre descriptivo: `fix/paginacion-de-tareas`, `feature/modo-solo-lectura`.
2. Mantenga los commits enfocados. Un commit resuelve un problema; varios commits sin relación se separan.
3. Use mensajes de commit convencionales: `feat:`, `fix:`, `docs:`, `test:`, `refactor:`, `chore:`.
4. Abra un pull request describiendo el cambio y el motivo.

## Requisitos del entorno

- .NET 10 SDK.
- PowerShell 7 o superior, para los scripts de publicación multiplataforma.

```sh
dotnet restore MCP.MicrosoftPlanner.slnx
dotnet build MCP.MicrosoftPlanner.slnx -c Release
dotnet test MCP.MicrosoftPlanner.slnx -c Release
```

## Reglas del proyecto

- **Idioma del código:** los identificadores, comentarios y textos técnicos van en inglés o español según la convención existente del archivo. La documentación para usuarios se escribe en español.
- **Formato:** el código se valida con `dotnet format MCP.MicrosoftPlanner.slnx --verify-no-changes --no-restore`. Ejecute el formateador antes de enviar.
- **Pruebas:** todo cambio de comportamiento observable necesita una prueba. Las pruebas no dependen de red ni de un tenant real: se simulan las respuestas de Microsoft Graph.
- **Errores:** las herramientas MCP devuelven errores con un código estable, nunca el detalle crudo de Graph ni un token.
- **Escrituras:** toda operación que modifica Planner exige confirmación explícita y, cuando aplica, el ETag del recurso.
- **Seguridad:** nunca se versionan secretos, tokens ni identificadores de una organización. La configuración real vive en el archivo del usuario.

## Flujo de trabajo de GitHub

Cada pull request debe:

- Describir el problema que resuelve y cómo se comprueba que quedó resuelto.
- Mantener las pruebas existentes en verde.
- Actualizar la documentación afectada en `docs/`.
- Añadir una entrada en `CHANGELOG.md` bajo la sección *Sin publicar*.

## Reportar problemas

Use las plantillas de issues. Un buen informe incluye:

- Versión del ejecutable y del runtime.
- Sistema operativo.
- Pasos para reproducir.
- Salida esperada y salida real.

## Seguridad

No reporte vulnerabilidades en una issue pública. Consulte [SECURITY.md](SECURITY.md).

## Código de conducta

Al participar acepta el [Código de conducta](CODE_OF_CONDUCT.md).
