# Eliminar Docker y Deployar a Azure Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Eliminar Docker del backend y dejar backend (BookIt-backend) y frontend (BookIt-frontend) configurados para deployar a Azure App Service vía push directo de código, automatizado con GitHub Actions.

**Architecture:** El backend (.NET 8) ya resuelve connection string y JWT secret desde variables de entorno — se le agrega CORS configurable de la misma forma y se le quita todo lo relacionado a contenedores. El frontend (Next.js 14) ya lee la URL del backend desde `NEXT_PUBLIC_API_BASE_URL` (variable de entorno, no hardcodeada) — se le quita un rewrite muerto que apuntaba a Render y se agrega build standalone para deployar a Azure App Service (Node) sin Docker. Cada repo obtiene su propio workflow de GitHub Actions que buildea y deploya a su App Service en cada push a `main`.

**Tech Stack:** .NET 8 / ASP.NET Core, Next.js 14, GitHub Actions, Azure App Service (Linux, .NET + Node), Azure Database for PostgreSQL Flexible Server.

## Global Constraints

- Ningún archivo de Docker debe quedar en el repo backend (`Dockerfile`, `docker-compose.yml`, `.dockerignore`).
- La base de datos (Azure Postgres Flexible Server) es la misma en dev y prod — no hay Postgres local ni docker-compose.
- Ningún secreto (connection string, JWT key, publish profiles) se commitea en el repo; todo vía GitHub Secrets / Azure Application Settings.
- No hay proyecto de tests automatizados en ninguno de los dos repos — la verificación de cada tarea es build exitoso + chequeo manual descrito en cada paso (no se introduce un framework de testing nuevo, está fuera de alcance).
- Cualquier `git push` o merge a `main`/`origin` requiere confirmación explícita del usuario antes de ejecutarse (toca estado remoto compartido).

---

## Backend (`BookIt-backend`, branch `chore/azure-deploy`, ya creada)

### Task 1: Eliminar artefactos de Docker y actualizar README

**Files:**
- Delete: `Dockerfile`
- Delete: `docker-compose.yml`
- Delete: `.dockerignore`
- Modify: `README.md`

**Interfaces:**
- Produces: repo backend sin ninguna referencia a Docker (verificado por grep en Task 1 Step 4).

- [ ] **Step 1: Borrar los archivos de Docker**

```bash
git rm Dockerfile docker-compose.yml .dockerignore
```

- [ ] **Step 2: Editar `README.md` — sacar la línea de Docker del stack técnico**

Reemplazar (línea 13):

```markdown
- **Docker** + **docker-compose** (API + Postgres) para levantar todo el entorno local con un solo comando
```

Por (eliminar la línea completa, sin reemplazo).

- [ ] **Step 3: Editar `README.md` — reemplazar la sección "Cómo correrlo localmente"**

Reemplazar todo el bloque desde `### Opción A — con Docker (recomendada, un solo comando)` hasta el final de `### Opción B — sin Docker` (antes del `## 👤 Autores`) por:

```markdown
### Cómo correrlo localmente

Requisitos: .NET 8 SDK. La base de datos es un único Azure Database for PostgreSQL Flexible Server (mismo para dev y prod) — no hace falta Postgres local.

```bash
git clone https://github.com/MatiOreiro/BookIt-backend.git
cd BookIt-backend
```

Crear `BookIt.API/.env` con la cadena de conexión al Flexible Server y una clave JWT de al menos 32 bytes:

```
ConnectionStrings__DefaultConnection=Host=<tu-servidor>.postgres.database.azure.com;Port=5432;Database=bookit;Username=<usuario>;Password=<password>;SSL Mode=Require;Trust Server Certificate=true
JwtSettings__SecretKey=reemplazar-con-una-clave-larga-y-aleatoria-de-al-menos-32-bytes
JwtSettings__Issuer=BookIt.API
JwtSettings__Audience=BookIt.Client
JwtSettings__ExpiresInMinutes=60
Cors__AllowedOrigins__0=http://localhost:3000
```

```bash
dotnet restore
dotnet run --project BookIt.API
```

Las migraciones de EF Core se aplican automáticamente al iniciar la app (`context.Database.MigrateAsync()` en `Bootstrapper.cs`), junto con un seed de categorías de evento fijas (Boda, Cumpleaños de XV, Cumpleaños, Evento corporativo, Bautismo, Graduación, Baile) si todavía no existen en la base.
```

- [ ] **Step 4: Verificar que no queda ninguna referencia a Docker**

Run: `grep -ril docker . --exclude-dir=bin --exclude-dir=obj --exclude-dir=.git`
Expected: sin resultados (ningún archivo contiene "docker").

- [ ] **Step 5: Confirmar que el proyecto sigue buildeando**

Run: `dotnet build BookIt-backend.sln`
Expected: `Build succeeded.`

- [ ] **Step 6: Commit**

```bash
git add -A
git commit -m "chore: eliminar Docker y actualizar README para correr contra Azure Postgres"
```

---

### Task 2: CORS configurable por entorno

**Files:**
- Modify: `BookIt.API/appsettings.json`
- Modify: `BookIt.API/appsettings.Development.json`
- Modify: `BookIt.API/Program.cs:104-118`

**Interfaces:**
- Consumes: nada de tareas anteriores.
- Produces: clave de configuración `Cors:AllowedOrigins` (array de strings), leída por `Program.cs`. En Azure se completa vía Application Setting `Cors__AllowedOrigins__0` con la URL del App Service del frontend (acción manual del usuario en el portal, fuera de este repo).

- [ ] **Step 1: Agregar la sección `Cors` a `appsettings.json`**

Reemplazar el contenido completo de `BookIt.API/appsettings.json` por:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "DefaultConnection": ""
  },
  "JwtSettings": {
    "SecretKey": "",
    "Issuer": "BookIt.API",
    "Audience": "BookIt.Client",
    "ExpiresInMinutes": "60"
  },
  "Cors": {
    "AllowedOrigins": []
  }
}
```

(Se quita el connection string local hardcodeado que quedaba en el `DefaultConnection` de base — ahora siempre viene de variables de entorno / `.env`, igual que `JwtSettings__SecretKey`.)

- [ ] **Step 2: Agregar la sección `Cors` a `appsettings.Development.json`**

Reemplazar el contenido completo de `BookIt.API/appsettings.Development.json` por:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "ConnectionStrings": {
    "DefaultConnection": ""
  },
  "JwtSettings": {
    "SecretKey": "",
    "Issuer": "BookIt.API",
    "Audience": "BookIt.Client",
    "ExpiresInMinutes": "120"
  },
  "Cors": {
    "AllowedOrigins": [
      "http://localhost:5173",
      "http://127.0.0.1:5173",
      "http://localhost:3000",
      "http://127.0.0.1:3000",
      "http://localhost:3001",
      "http://127.0.0.1:3001"
    ]
  }
}
```

(Se saca también `ConnectionStrings:LocalConnection`, que apuntaba a la Postgres local que ya no existe.)

- [ ] **Step 3: Leer los orígenes desde configuración en `Program.cs`**

Reemplazar (líneas 104-118):

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
            policy.WithOrigins(
                "http://localhost:5173",
                "http://127.0.0.1:5173",
                "http://localhost:3000",
                "http://127.0.0.1:3000",
                "http://localhost:3001",
                "http://127.0.0.1:3001")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});
```

Por:

```csharp
var corsAllowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>() ?? Array.Empty<string>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy.WithOrigins(corsAllowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});
```

- [ ] **Step 4: Confirmar que el proyecto buildea**

Run: `dotnet build BookIt-backend.sln`
Expected: `Build succeeded.`

- [ ] **Step 5: Verificación manual de CORS en dev**

Con `BookIt.API/.env` apuntando al Flexible Server de Azure:

Run: `dotnet run --project BookIt.API` (queda escuchando en `http://localhost:5062`)

En otra terminal:

```bash
curl -i -H "Origin: http://localhost:3000" -H "Access-Control-Request-Method: GET" -X OPTIONS http://localhost:5062/departamentos
```

Expected: respuesta con header `Access-Control-Allow-Origin: http://localhost:3000`.

```bash
curl -i -H "Origin: http://evil.example.com" -H "Access-Control-Request-Method: GET" -X OPTIONS http://localhost:5062/departamentos
```

Expected: respuesta **sin** el header `Access-Control-Allow-Origin`.

- [ ] **Step 6: Commit**

```bash
git add BookIt.API/appsettings.json BookIt.API/appsettings.Development.json BookIt.API/Program.cs
git commit -m "feat: hacer configurable la lista de orígenes CORS"
```

---

### Task 3: Workflow de GitHub Actions para deploy del backend

**Files:**
- Create: `.github/workflows/azure-deploy.yml`

**Interfaces:**
- Consumes: GitHub Secrets `AZURE_BACKEND_APP_NAME` y `AZURE_BACKEND_PUBLISH_PROFILE` (el usuario los crea manualmente en GitHub → Settings → Secrets, con el nombre del App Service y el publish profile descargado desde el portal de Azure; fuera de alcance de este repo).

- [ ] **Step 1: Crear el workflow**

```yaml
name: Deploy backend to Azure App Service

on:
  push:
    branches: [main]

jobs:
  build-and-deploy:
    runs-on: ubuntu-latest
    steps:
      - name: Checkout
        uses: actions/checkout@v4

      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '8.0.x'

      - name: Restore
        run: dotnet restore BookIt.API/BookIt.API.csproj

      - name: Publish
        run: dotnet publish BookIt.API/BookIt.API.csproj -c Release -o ./publish

      - name: Deploy to Azure App Service
        uses: azure/webapps-deploy@v3
        with:
          app-name: ${{ secrets.AZURE_BACKEND_APP_NAME }}
          publish-profile: ${{ secrets.AZURE_BACKEND_PUBLISH_PROFILE }}
          package: ./publish
```

- [ ] **Step 2: Validar la sintaxis del YAML**

Run: `python -c "import yaml; yaml.safe_load(open('.github/workflows/azure-deploy.yml'))" && echo OK`
Expected: `OK` (si `python`/`pyyaml` no están disponibles en la máquina, revisar visualmente la indentación contra el bloque de arriba — es una copia exacta).

- [ ] **Step 3: Commit**

```bash
git add .github/workflows/azure-deploy.yml
git commit -m "ci: agregar workflow de GitHub Actions para deploy a Azure App Service"
```

> Nota (fuera de este repo, acción manual del usuario): crear los secrets `AZURE_BACKEND_APP_NAME` y `AZURE_BACKEND_PUBLISH_PROFILE` en GitHub, y las Application Settings en el App Service de Azure (`ConnectionStrings__DefaultConnection`, `JwtSettings__SecretKey`, `JwtSettings__Issuer`, `JwtSettings__Audience`, `JwtSettings__ExpiresInMinutes`, `Cors__AllowedOrigins__0`). El workflow recién deploya de verdad en el próximo push a `main` (después del merge de esta branch).

---

## Frontend (`BookIt-frontend`)

### Task 4: Housekeeping de git — cerrar la feature branch antes de abrir la de infra

**Files:**
- Modify (borrado ya pendiente en el working tree): `.env.example`
- Untracked (ya existentes en el working tree): `docs/superpowers/plans/2026-06-23-user-avatar-menu.md`, `docs/superpowers/specs/2026-06-23-user-avatar-menu-design.md`

**Interfaces:**
- Produces: `main` actualizado en `origin`, punto de partida limpio para `chore/azure-deploy` en Task 5.

- [ ] **Step 1: Revisar qué se va a commitear**

Run: `git status`
Expected: `deleted: .env.example` + los dos archivos untracked de arriba, nada más.

- [ ] **Step 2: Stagear y commitear**

```bash
git add -A
git commit -m "chore: eliminar .env.example obsoleto y agregar specs de user-avatar-menu"
```

- [ ] **Step 3: ⚠️ CONFIRMACIÓN REQUERIDA antes de continuar**

Mostrarle al usuario el resultado de `git log --oneline -1` y `git status` y pedir confirmación explícita antes de ejecutar el push de Step 4 — esta acción toca `origin` y la branch `main` compartida.

- [ ] **Step 4: Push de la feature branch**

```bash
git push origin feature/rf027-propuesta-evento
```

- [ ] **Step 5: Merge local a `main` y push**

```bash
git checkout main
git pull origin main
git merge --no-ff feature/rf027-propuesta-evento -m "Merge feature/rf027-propuesta-evento into main"
git push origin main
```

Expected: `main` local y remoto quedan sincronizados, incluyendo los commits de la feature branch.

---

### Task 5: Branch de infra + limpiar rewrite muerto hacia Render

**Files:**
- Create branch: `chore/azure-deploy` (desde `main`, después de Task 4)
- Modify: `next.config.mjs`

**Interfaces:**
- Consumes: `main` actualizado (Task 4).
- Produces: `next.config.mjs` con `output: 'standalone'`, consumido por el workflow de Task 6 (empaqueta `.next/standalone`).

- [ ] **Step 1: Crear la branch de infra**

```bash
git checkout -b chore/azure-deploy
```

- [ ] **Step 2: Sacar el rewrite muerto hacia Render y activar build standalone**

`next.config.mjs:9-15` define un rewrite (`/api/backend/:path*` → `https://bookit-backend-es10.onrender.com`) que no se usa en ningún lado del código (el cliente HTTP real es `src/api/axiosClient.ts`, que ya lee la URL del backend desde `NEXT_PUBLIC_API_BASE_URL`). Se elimina por quedar apuntando a un backend que se está dando de baja.

Reemplazar el archivo completo por:

```javascript
import path from 'path';
import { fileURLToPath } from 'url';

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);

const nextConfig = {
  output: 'standalone',

  webpack: (config) => {
    config.resolve = config.resolve || {};
    config.resolve.alias = {
      ...(config.resolve.alias || {}),
      'react-router/dom': path.resolve(__dirname, 'node_modules/react-router/dist/development/dom-export.js'),
      'react-router': path.resolve(__dirname, 'node_modules/react-router/dist/development/index.js'),
    };

    return config;
  },
};

export default nextConfig;
```

- [ ] **Step 3: Confirmar que el build funciona**

Run: `npm run build`
Expected: build exitoso, y se genera la carpeta `.next/standalone/`.

- [ ] **Step 4: Commit**

```bash
git add next.config.mjs
git commit -m "chore: sacar rewrite muerto hacia Render y activar output standalone para Azure"
```

---

### Task 6: Workflow de GitHub Actions para deploy del frontend

**Files:**
- Create: `.github/workflows/azure-deploy.yml`

**Interfaces:**
- Consumes: `output: 'standalone'` de `next.config.mjs` (Task 5). GitHub Secrets `AZURE_FRONTEND_APP_NAME`, `AZURE_FRONTEND_PUBLISH_PROFILE`, `NEXT_PUBLIC_API_BASE_URL` (el usuario los crea manualmente en GitHub; `NEXT_PUBLIC_API_BASE_URL` es la URL pública del App Service del backend, fuera de alcance de este repo).

- [ ] **Step 1: Crear el workflow**

```yaml
name: Deploy frontend to Azure App Service

on:
  push:
    branches: [main]

jobs:
  build-and-deploy:
    runs-on: ubuntu-latest
    env:
      NEXT_PUBLIC_API_BASE_URL: ${{ secrets.NEXT_PUBLIC_API_BASE_URL }}
    steps:
      - name: Checkout
        uses: actions/checkout@v4

      - name: Setup Node
        uses: actions/setup-node@v4
        with:
          node-version: '20'

      - name: Install dependencies
        run: npm ci

      - name: Build
        run: npm run build

      - name: Assemble standalone package
        run: |
          cp -r public .next/standalone/public
          cp -r .next/static .next/standalone/.next/static

      - name: Deploy to Azure App Service
        uses: azure/webapps-deploy@v3
        with:
          app-name: ${{ secrets.AZURE_FRONTEND_APP_NAME }}
          publish-profile: ${{ secrets.AZURE_FRONTEND_PUBLISH_PROFILE }}
          package: .next/standalone
```

- [ ] **Step 2: Validar la sintaxis del YAML**

Run: `python -c "import yaml; yaml.safe_load(open('.github/workflows/azure-deploy.yml'))" && echo OK`
Expected: `OK` (mismo fallback manual que en el backend si `python`/`pyyaml` no están disponibles).

- [ ] **Step 3: Commit**

```bash
git add .github/workflows/azure-deploy.yml
git commit -m "ci: agregar workflow de GitHub Actions para deploy a Azure App Service"
```

> Nota (fuera de este repo, acción manual del usuario): crear los secrets en GitHub, y en el App Service de Azure configurar el startup command `node server.js` (es como corre una build standalone de Next.js) y, si hiciera falta en runtime, las Application Settings equivalentes a las de `BookIt-frontend/.env` (`NEXT_PUBLIC_CLOUDINARY_CLOUD_NAME`, `NEXT_PUBLIC_CLOUDINARY_UPLOAD_PRESET` — estas dos son `NEXT_PUBLIC_`, así que además de estar en runtime deben estar seteadas en el build de GitHub Actions si su valor cambia entre entornos).

---

## Al finalizar

Ambas branches (`chore/azure-deploy` en backend y en frontend) quedan listas para PR/merge a `main` — pedirle confirmación al usuario antes de mergear, igual que en Task 4.
