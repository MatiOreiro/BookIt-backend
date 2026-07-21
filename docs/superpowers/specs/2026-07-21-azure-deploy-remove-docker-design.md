# Eliminar Docker y deployar a Azure (backend + frontend)

## Contexto

El backend (BookIt.API, .NET 8) corre hoy con Docker/docker-compose para levantar la API junto a una Postgres local. El frontend (Next.js) apunta hardcodeado a un backend deployado en Render (`bookit-backend-es10.onrender.com`).

Se decidió migrar el deploy a Azure, sacando Docker por completo y usando push directo de código (App Service nativo) con CI/CD vía GitHub Actions. La base de datos pasa a ser un único Azure Database for PostgreSQL Flexible Server, usado tanto en desarrollo como en producción (sin Postgres local).

Recursos de Azure: la base de datos ya existe; los App Services (backend y frontend) los está creando el usuario.

## Objetivo

1. Eliminar todo rastro de Docker del repo backend.
2. Dejar el backend y el frontend configurados para correr en Azure App Service vía variables de entorno / Application Settings (sin contenedores).
3. Automatizar el deploy con GitHub Actions en ambos repos.

## Cambios — Backend (BookIt-backend)

### Eliminar Docker
- Borrar `Dockerfile`, `docker-compose.yml`, `.dockerignore`.
- En `README.md`: eliminar la sección "Opción A — con Docker" y actualizar "Opción B — sin Docker" como único camino de setup local, indicando que se conecta contra el Azure Postgres Flexible Server (no hay Postgres local).

### CORS configurable
- `Program.cs` hoy hardcodea una lista de orígenes `localhost` en la policy "Frontend" (líneas 104-118). Se cambia para leer la lista de orígenes permitidos desde configuración (`appsettings.json` clave `Cors:AllowedOrigins`, override-able por Application Settings en Azure), incluyendo ahí la URL del App Service del frontend.

### Configuración de secretos
- Sin cambios de código: `Program.cs` ya resuelve `ConnectionStrings__DefaultConnection` y `JwtSettings__SecretKey` desde variables de entorno (compatible nativamente con Application Settings de Azure App Service, que usa `__` como separador de sección).
- En Azure App Service (Application Settings) configurar manualmente (fuera de este repo, acción del usuario en el portal):
  - `ConnectionStrings__DefaultConnection` → connection string del Flexible Server.
  - `JwtSettings__SecretKey`, `JwtSettings__Issuer`, `JwtSettings__Audience`, `JwtSettings__ExpiresInMinutes`.
  - `Cors__AllowedOrigins__0` → URL del App Service del frontend.

### GitHub Actions
- Nuevo workflow `.github/workflows/azure-deploy.yml`:
  - Trigger: push a `main`.
  - Steps: `dotnet restore` → `dotnet publish -c Release` → deploy a Azure App Service (`azure/webapps-deploy@v3`), usando el publish profile del App Service como GitHub Secret (`AZURE_BACKEND_PUBLISH_PROFILE`).

## Cambios — Frontend (BookIt-frontend)

### Backend URL configurable
- El cliente HTTP real (`src/api/axiosClient.ts`) ya lee la URL del backend desde `NEXT_PUBLIC_API_BASE_URL` (variable de entorno, no hardcodeada). No hace falta cambiar código para esto.
- `next.config.mjs:9-15` define un rewrite (`/api/backend/:path*` → `https://bookit-backend-es10.onrender.com`) que no se usa en ningún lado del código — queda muerto apuntando al backend de Render que se da de baja. Se elimina.
- Se agrega `output: 'standalone'` a `next.config.mjs` para poder deployar a Azure App Service (Node) sin Docker con un artefacto autocontenido.

### GitHub Actions
- Nuevo workflow `.github/workflows/azure-deploy.yml`:
  - Trigger: push a `main`.
  - Steps: `npm ci` → `npm run build` (con `NEXT_PUBLIC_API_BASE_URL` inyectada como GitHub Secret, ya que Next.js la hornea en el bundle de cliente en build time) → armar el paquete standalone → deploy a Azure App Service (Node), usando el publish profile como GitHub Secret (`AZURE_FRONTEND_PUBLISH_PROFILE`).

## Flujo de git

- **Backend**: rama `chore/azure-deploy` creada desde `main` (limpio). Todo el trabajo de este spec se commitea ahí; se mergea a `main` al finalizar (decisión de merge pendiente de confirmación del usuario en su momento).
- **Frontend**: antes de crear la rama de este trabajo, se commitean y pushean los cambios pendientes en `feature/rf027-propuesta-evento` (borrado de `.env.example` + docs sueltos), se mergea localmente a `main`, y se pushea `main` a `origin` — con confirmación explícita del usuario antes de cada push/merge por tocar el remoto. Recién ahí se crea `chore/azure-deploy` desde `main` actualizado.

## Fuera de alcance

- Creación de los recursos de Azure (App Services, Flexible Server) vía portal/CLI — lo está haciendo el usuario directamente en Azure.
- Migración de datos existentes desde Render/otra fuente a Azure Postgres, si aplica.
- Configuración de dominio custom / HTTPS custom cert en los App Services.

## Testing / validación

- Backend: `dotnet build` local sin Docker; confirmar que arranca contra el Flexible Server usando `.env` o variables de entorno locales.
- Frontend: `npm run build` local con `BACKEND_URL` apuntando al App Service de backend.
- Post-deploy: smoke test manual — login, listado de servicios, creación de reserva — contra las URLs de Azure.
