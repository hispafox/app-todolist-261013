# AppTodoList

MVP didáctico para gestionar una lista compartida de tareas. La aplicación separa una API ASP.NET Core 10, una SPA React/TypeScript y persistencia SQLite con Entity Framework Core.

## Alcance

- Crear, consultar, editar, cambiar el estado y eliminar tareas.
- Buscar por título o descripción; filtrar por estado y prioridad.
- Ordenar por creación, vencimiento, prioridad o estado y recorrer resultados paginados.
- Validar entradas en el servidor; mostrar carga, lista vacía, errores y confirmaciones en la web.
- Valores iniciales: estado `Pending` y prioridad `Medium`. La API devuelve fechas en UTC.

No hay cuentas ni autenticación: todas las personas con acceso ven y pueden cambiar la misma lista. No publiques el servicio en una red no confiable ni guardes datos sensibles.

## Requisitos

- .NET 10 SDK
- Node.js 22 o superior y npm

## Ejecutar en local

Inicia la API en una terminal desde la raíz del repositorio:

```powershell
dotnet run --project src\AppTodoList.Api
```

En Development, la API aplica las migraciones versionadas al arrancar y crea `src\AppTodoList.Api\AppTodoList.db`. Está disponible en `http://localhost:5297`; OpenAPI se publica en `/openapi/v1.json` y el estado de la API y SQLite en `/health`.

Inicia la web en otra terminal:

```powershell
Set-Location src\AppTodoList.Web
npm install
npm run dev
```

Abre `http://localhost:5173`. Vite reenvía `/api` a la API local; no se necesita configurar una URL ni exponer secretos en el navegador.

## Verificación

Desde la raíz:

```powershell
dotnet test AppTodoList.sln
Set-Location src\AppTodoList.Web
npm test
npm run lint
npm run build
```

Las pruebas de integración del backend usan una base SQLite en memoria aislada; las pruebas de componentes simulan la API.

## Migraciones y despliegue

En producción no se aplican migraciones automáticamente. Con `ConnectionStrings__AppTodoList`, `Cors__AllowedOrigins__0` y `ASPNETCORE_ENVIRONMENT=Production` configurados en el entorno, aplica el esquema de forma explícita:

```powershell
dotnet ef database update --project src\AppTodoList.Api --startup-project src\AppTodoList.Api
```

Termina TLS en un proxy HTTPS, limita CORS a los orígenes aprobados y conserva el archivo SQLite en almacenamiento persistente con copias de seguridad. La aplicación no incluye autenticación ni mecanismos de recuperación.

## Más información

- [Análisis funcional y técnico](docs/analisis-completo-app-todolist.md)
- [Documentación](docs/README.md)
- [Laboratorios](labs/)
- [Instrucciones de GitHub Copilot](.github/copilot-instructions.md)
