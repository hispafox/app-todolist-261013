# Herramientas del MCP de GitHub

Este documento describe las herramientas de GitHub MCP que aparecen disponibles en la sesión de Copilot de este repositorio. Se muestran dos instancias del servidor; algunas herramientas se repiten entre ambas y sus capacidades no son idénticas. El catálogo puede cambiar si se actualiza o reconfigura el servidor.

Las herramientas de lectura consultan datos de GitHub. Las herramientas de escritura pueden modificar repositorios, incidencias y pull requests; comprueba sus parámetros y el destino antes de usarlas. En particular, eliminar un repositorio es una operación destructiva.

## Servidor `github-mcp-server`

Los nombres de esta sección se muestran con el prefijo `github-mcp-server-`.

| Herramienta | Descripción |
|---|---|
| `actions_get` | Consulta detalles de una ejecución de GitHub Actions. |
| `actions_list` | Lista ejecuciones o flujos de trabajo de GitHub Actions disponibles en un repositorio. |
| `get_commit` | Obtiene los detalles de un commit. |
| `get_copilot_space` | Consulta la información de un espacio de GitHub Copilot. |
| `get_file_contents` | Lee el contenido de un archivo o enumera los elementos de un directorio del repositorio. |
| `get_job_logs` | Obtiene los registros de un trabajo de GitHub Actions. |
| `issue_read` | Consulta una incidencia y sus datos disponibles. |
| `list_branches` | Lista las ramas del repositorio. |
| `list_commits` | Lista commits del repositorio, con filtros disponibles. |
| `list_issues` | Lista incidencias del repositorio. |
| `list_pull_requests` | Lista pull requests del repositorio. |
| `pull_request_read` | Consulta un pull request, por ejemplo sus cambios, commits, revisiones, comentarios o comprobaciones. |
| `search_code` | Busca código en repositorios accesibles. |
| `search_issues` | Busca incidencias mediante una consulta en lenguaje natural. |
| `search_pull_requests` | Busca pull requests. |
| `search_repositories` | Busca repositorios de GitHub. |
| `search_users` | Busca usuarios de GitHub. |

## Servidor `io.github.github/github-mcp-server`

En esta sesión los nombres de las herramientas se exponen con el prefijo `io-github-github-github-mcp-server-`. En la tabla se muestra la parte específica del nombre.

### Incidencias y comentarios

| Herramienta | Descripción |
|---|---|
| `issue_read` | Lee una incidencia, sus comentarios o etiquetas, y consulta sus relaciones de incidencia padre/subincidencias. |
| `issue_write` | Crea o actualiza una incidencia; puede establecer título, cuerpo, estado, asignados, etiquetas, tipo, hito y campos compatibles. |
| `list_issues` | Lista las incidencias del repositorio, con los filtros y la paginación admitidos. |
| `add_issue_comment` | Añade un comentario o una reacción a una incidencia o pull request. |
| `update_issue_comment` | Edita un comentario existente de una incidencia o pull request. |
| `sub_issue_write` | Añade, quita o cambia el orden de una subincidencia dentro de una jerarquía. |
| `list_issue_fields` | Lista los campos personalizados de incidencias disponibles para el repositorio u organización. |
| `list_issue_types` | Lista los tipos de incidencia admitidos. |
| `assign_copilot_to_issue` | Asigna una incidencia al agente de programación de Copilot para que trabaje en ella. |

### Pull requests y revisiones

| Herramienta | Descripción |
|---|---|
| `pull_request_read` | Consulta un pull request, su diff, archivos, commits, revisiones, comentarios de revisión, comentarios generales o comprobaciones. |
| `list_pull_requests` | Lista los pull requests del repositorio, con los filtros y la paginación admitidos. |
| `create_pull_request` | Crea un pull request entre una rama de origen y otra de destino. |
| `update_pull_request` | Actualiza los datos de un pull request. |
| `merge_pull_request` | Fusiona un pull request mediante merge, squash o rebase. |
| `update_pull_request_branch` | Actualiza la rama del pull request con los cambios de su rama base. |
| `pull_request_review_write` | Crea, envía o elimina una revisión pendiente, y resuelve o vuelve a abrir un hilo de revisión. |
| `add_comment_to_pending_review` | Añade un comentario a una revisión pendiente de un pull request. |
| `add_reply_to_pull_request_com` | Responde a un comentario de revisión de un pull request o añade una reacción. |
| `request_copilot_review` | Solicita una revisión automática de Copilot para un pull request. |
| `create_pull_request_with_copi` | Encarga una tarea al agente de programación de Copilot; el agente trabaja en segundo plano y crea un pull request con los cambios. Este es el nombre expuesto en esta sesión. |

### Repositorios, ramas y archivos

| Herramienta | Descripción |
|---|---|
| `create_repository` | Crea un repositorio en la cuenta personal o en una organización. |
| `delete_repository` | Elimina un repositorio. Es una operación destructiva. |
| `fork_repository` | Crea una bifurcación de un repositorio en la cuenta u organización indicada. |
| `create_branch` | Crea una rama a partir de otra rama del repositorio. |
| `list_branches` | Lista las ramas del repositorio. |
| `create_or_update_file` | Crea o actualiza un archivo en una rama. Al actualizar, requiere el SHA actual del archivo. |
| `delete_file` | Elimina un archivo de una rama del repositorio. |
| `push_files` | Añade o actualiza varios archivos en una rama en una sola operación. |
| `get_file_contents` | Lee el contenido de un archivo o enumera los elementos de un directorio. |
| `get_commit` | Obtiene los detalles de un commit. |
| `list_commits` | Lista commits de una rama o revisión, con filtros opcionales. |
| `search_code` | Busca texto o código en repositorios accesibles. |
| `search_commits` | Busca commits en GitHub. |
| `search_repositories` | Busca repositorios de GitHub. |

### Etiquetas, versiones, equipos y colaboradores

| Herramienta | Descripción |
|---|---|
| `get_label` | Obtiene los detalles de una etiqueta del repositorio. |
| `list_tags` | Lista las etiquetas Git del repositorio. |
| `get_tag` | Obtiene los detalles de una etiqueta Git. |
| `list_releases` | Lista las versiones publicadas del repositorio. |
| `get_latest_release` | Obtiene la versión más reciente del repositorio. |
| `get_release_by_tag` | Obtiene una versión identificada por su etiqueta. |
| `get_teams` | Lista los equipos de una organización a los que se puede acceder. |
| `get_team_members` | Lista los integrantes de un equipo. |
| `list_repository_collaborators` | Lista colaboradores del repositorio y los permisos disponibles para cada uno. |

### Usuario, Copilot y seguridad

| Herramienta | Descripción |
|---|---|
| `get_me` | Obtiene el perfil de la cuenta de GitHub autenticada. |
| `get_copilot_job_status` | Consulta el estado de un trabajo delegado al agente de programación de Copilot. |
| `run_secret_scanning` | Busca secretos en el contenido de archivos, fragmentos o diffs que se proporcionan a la herramienta; no recibe rutas para escanear por sí sola todo el repositorio. |
| `search_issues` | Busca incidencias mediante una consulta en lenguaje natural. |
| `search_pull_requests` | Busca pull requests. |
| `search_users` | Busca usuarios de GitHub. |

## Consideraciones

- La presencia de una herramienta en este catálogo indica que está expuesta por la sesión; no garantiza que la cuenta tenga permiso para todas las operaciones.
- Las operaciones de escritura solo deben usarse cuando se haya solicitado el cambio correspondiente y se haya confirmado el repositorio y el objeto afectados.
- Algunas herramientas aparecen en las dos instancias. Por ejemplo, ambas exponen lectura de archivos, commits, incidencias, pull requests y búsquedas, pero una instancia puede ofrecer operaciones adicionales que la otra no tiene.
