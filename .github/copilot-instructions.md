# Instrucciones de Copilot para AppTodoList

## Contexto y stack

- AppTodoList es una aplicación de lista de tareas que debe poder evolucionar desde un MVP hasta producción.
- Backend: ASP.NET Core 10 Web API con controladores REST.
- Frontend: React + Vite + TypeScript; no usar Razor Pages ni MVC Views.
- Persistencia: SQLite mediante Entity Framework Core (EF Core).
- Consulta [el análisis funcional y técnico](../docs/analisis-completo-app-todolist.md) para ampliar el alcance y el roadmap. Si contradice el código o una decisión explícita más reciente, prioriza el código y esa decisión.

## Arquitectura

- Antes de cambiar código, inspecciona la estructura y los patrones existentes. En un repositorio nuevo, separa el backend API y la aplicación web según la estructura ya documentada.
- Mantén responsabilidades claras: controlador HTTP → DTO/validación → servicio o lógica de aplicación → `DbContext`/EF Core → SQLite.
- Los controladores traducen HTTP y delegan la lógica de negocio; no contienen reglas de negocio ni consultas complejas.
- Usa inyección de dependencias por constructor. No instancies manualmente servicios o contextos registrados en el contenedor.
- No añadas capas, repositorios genéricos, bibliotecas o patrones si no resuelven una necesidad real o no siguen una convención ya adoptada.
- Mantén la API REST independiente de la UI: React consume JSON a través de servicios cliente, no accede directamente a la base de datos.

## Convenciones de código

- Escribe explicaciones, documentación y mensajes visibles para la persona usuaria en castellano.
- Usa castellano para nombres de clases, métodos, variables y propiedades de dominio nuevos, de forma consistente con el lab del proyecto. Conserva nombres existentes al modificarlos, salvo que se solicite un renombrado.
- Sigue las convenciones idiomáticas de cada lenguaje: PascalCase en tipos y componentes React; camelCase en funciones, variables, propiedades JSON y archivos TypeScript cuando corresponda.
- Usa interfaces con prefijo `I` cuando una abstracción sea necesaria (por ejemplo, `ITareaServicio`).
- Mantén nullable reference types en C# y el modo estricto de TypeScript; evita `null`/`undefined` no controlados, `any` y supresiones de errores de tipos.
- Usa `async`/`await` para operaciones de E/S, especialmente consultas y cambios de EF Core. Propaga `CancellationToken` en operaciones de API y persistencia cuando la firma lo permita.
- Valida los datos en el backend, aunque también exista validación de formularios en el frontend. Devuelve errores HTTP claros y coherentes; no ocultes excepciones ni devuelvas resultados de éxito ante fallos.
- Mantén los cambios acotados a lo solicitado y actualiza la documentación relacionada cuando cambie el comportamiento o los contratos.

## Dominio de tareas

- El modelo evoluciona alrededor de tareas con título, descripción, estado, prioridad, fechas de creación/actualización y vencimiento; el usuario y la categoría son relaciones opcionales según el alcance implementado.
- Los estados funcionales previstos son pendiente, en progreso y completada; las prioridades previstas son baja, media y alta. Reutiliza las enumeraciones y serialización existentes, y no introduzcas valores incompatibles.
- Una tarea asociada a un usuario solo puede consultarse o modificarse dentro del ámbito autorizado de ese usuario cuando la autenticación esté implementada.
- No añadas colaboración, notificaciones, informes u otras funciones del roadmap si no forman parte de la solicitud actual.

## API y contratos

- Sigue los endpoints REST ya definidos y las convenciones del proyecto. El análisis propone `/api/tasks`, `/api/categories` y `/api/auth`, con operaciones HTTP apropiadas para consultar, crear, actualizar y eliminar.
- Usa DTOs de entrada y salida; no expongas directamente entidades EF Core ni propiedades internas de persistencia.
- Mantén los contratos JSON en camelCase y conserva las rutas, nombres de campos y valores ya publicados. Cualquier cambio incompatible requiere explicitar y actualizar todos los consumidores y documentación.
- Usa códigos de estado HTTP adecuados, respuestas consistentes y filtros/paginación en listados cuando el volumen lo justifique.

## EF Core y SQLite

- Configura SQLite mediante configuración por entorno e inyección de `DbContext`; no incrustes cadenas de conexión en código.
- Gestiona cambios de esquema con migraciones de EF Core versionadas. No sustituyas migraciones por `EnsureCreated` en la aplicación.
- Configura relaciones, restricciones e índices relevantes para las consultas reales (por ejemplo, usuario, estado, prioridad y vencimiento, cuando esos campos existan).
- Usa `AsNoTracking` en lecturas que no requieran edición y proyecta a DTOs en las consultas. Filtra y pagina en la base de datos; evita cargar colecciones completas para filtrar en memoria.
- Usa las APIs asíncronas de EF Core y evita SQL concatenado. Si se requiere SQL manual, parametriza valores.
- No cambies SQLite por otro proveedor ni agregues una abstracción de proveedor sin solicitud explícita. Ten en cuenta que el archivo SQLite y sus copias de seguridad necesitan almacenamiento persistente y una estrategia de respaldo adecuada para producción.

## React, Vite y TypeScript

- Mantén componentes enfocados y reutilizables; separa presentación, estado y comunicación HTTP según la estructura existente.
- Centraliza el acceso a la API fuera de los componentes de presentación y representa estados de carga, error, vacío y éxito.
- Los formularios deben ofrecer mensajes comprensibles y validación accesible; la validación del cliente nunca reemplaza la validación del servidor.
- Reutiliza las dependencias, el gestor de estado, el enrutador y el sistema de estilos ya instalados. No incorpores una biblioteca nueva para una necesidad trivial.
- No guardes secretos en variables `VITE_*`: esas variables se exponen al navegador.

## Seguridad y producción

- No añadas, imprimas ni confirmes contraseñas, tokens, claves o secretos. Usa User Secrets en desarrollo y variables/gestión segura de configuración en despliegues.
- En producción, usa HTTPS, CORS con orígenes explícitos, errores sin stack traces públicos y logging estructurado sin credenciales ni datos sensibles.
- Si se implementa autenticación, usa mecanismos probados de ASP.NET Core, hash seguro de contraseñas y autorización en servidor. No inventes un esquema propio ni des por decidido JWT, Identity, refresh tokens o roles si el código y la solicitud no lo establecen.
- Protege las operaciones por propiedad de los datos; no confíes en identificadores enviados por el cliente para autorizar acceso.
- Para cambios de producción que afecten la base de datos, configuración, backups o despliegue, documenta los pasos de migración, recuperación y verificación pertinentes.

## Pruebas y comandos

- Revisa los proyectos de tests, `*.csproj`, `package.json` y scripts existentes antes de elegir o ejecutar comandos.
- Añade o actualiza pruebas para el comportamiento modificado: lógica de dominio/servicios y API en backend; componentes o flujos afectados en frontend.
- Ejecuta las pruebas, compilaciones y linters más pequeños que cubran el cambio. No inventes scripts ni declares una validación exitosa si no se ejecutó.
- Mantén pruebas de integración de persistencia aisladas de la base SQLite de desarrollo para que no alteren datos de la persona usuaria.

## Alcance de los cambios

- Resuelve la causa del problema con el cambio más sencillo que respete estas convenciones y el comportamiento existente.
- No hagas refactorizaciones ni actualizaciones de paquetes ajenas a la solicitud.
- Si falta una decisión que afecte contratos, autenticación, datos o despliegue, inspecciona primero el código y la documentación; pregunta antes de introducir un cambio incompatible o difícil de revertir.
