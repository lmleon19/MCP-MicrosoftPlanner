## Descripción

Explique qué cambia y qué problema resuelve.

Closes #

## Tipo de cambio

- [ ] Corrección
- [ ] Funcionalidad nueva
- [ ] Documentación
- [ ] Refactorización
- [ ] Pruebas

## Verificación

- [ ] `dotnet build MCP.MicrosoftPlanner.slnx -c Release` sin errores ni advertencias nuevas
- [ ] `dotnet test MCP.MicrosoftPlanner.slnx -c Release` en verde
- [ ] `dotnet format MCP.MicrosoftPlanner.slnx --verify-no-changes --no-restore` correcto
- [ ] Pruebas nuevas o ajustadas para el comportamiento cambiado
- [ ] Documentación actualizada en `docs/`
- [ ] Entrada en `CHANGELOG.md` bajo *Sin publicar*

## Alcance de permisos

Si el cambio agrega o modifica llamadas a Microsoft Graph, indique los permisos delegados requeridos y por qué no bastan los existentes.

## Sin secretos

- [ ] El cambio no incluye tokens, secretos ni identificadores de una organización
