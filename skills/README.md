# Skills del proyecto

Esta carpeta aloja las skills locales del proyecto. Cada skill vive en un subdirectorio
con su archivo `SKILL.md` y se declara en `opencode.json` bajo `skills`.

En este repositorio no hay skills locales: el catálogo de trabajo se carga desde el
harness instalado en la máquina del desarrollador, y `opencode.json` declara `./skills`
para que un contributedor pueda agregar las suyas sin tocar la configuración global.

Para agregar una skill:

1. Cree `skills/<nombre>/SKILL.md` con el frontmatter y el contrato que el harness exige.
2. Verifique con `dotnet format MCP.MicrosoftPlanner.slnx --verify-no-changes --no-restore`
   y `dotnet test MCP.MicrosoftPlanner.slnx -c Release`.
3. Documente el cambio en `CHANGELOG.md`.

Este archivo existe para que Git conserve la carpeta, que de otro modo no versiona
directorios vacíos.
