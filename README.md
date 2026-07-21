# 📅 Bookit — Backend

> Este repo es el **backend** de Bookit. El frontend vive en un repo aparte → **[BookIt-frontend](https://github.com/MatiOreiro/BookIt-frontend)**

API REST del marketplace de reserva de salones de eventos y servicios complementarios en el mercado uruguayo. Proyecto de tesis (capstone) de **Analista en Tecnologías de la Información**, Universidad ORT Uruguay.

## 🛠️ Stack técnico

- **.NET 8** — ASP.NET Core Web API
- **Entity Framework Core 8** + **Npgsql** — ORM, migraciones Code First
- **PostgreSQL**
- **JWT Bearer** (HS256) para autenticación, con **BCrypt** para hash de contraseñas

## 🏗️ Arquitectura

Arquitectura en capas dentro de un único proyecto (`BookIt.API`):

```
Controllers/    # Endpoints HTTP
Services/       # Lógica de negocio (con interfaces, inyección de dependencias)
Repositories/   # Acceso a datos (con interfaces)
Models/         # Entidades de dominio
DTOs/           # Contratos de entrada/salida de la API
Data/           # DbContext
Migrations/     # Migraciones de EF Core
Middleware/     # Manejo global de excepciones
Infrastructure/ # Bootstrapping (carga de .env, seed de datos, resolución de connection string)
```

## ✨ Funcionalidades / endpoints principales

| Recurso | Endpoints | Qué hace |
|---|---|---|
| **Auth** | `POST /auth/register`, `/register-vendor`, `/login` | Registro de usuarios y proveedores, login con JWT |
| **Users** | `GET /users`, `/users/{id}`, `/users/me`, `POST /users/me/change-password`, `PUT /users/me/profile-image` | Gestión de perfil |
| **Services** | `GET /services`, `/active`, `/{id}`, `/search`, `/filter`, `/vendor/{vendorId}`, `POST/PUT/DELETE /services`, `/{salonId}/servicios-asociados` | CRUD de salones/servicios, búsqueda, filtros, servicios asociados a un salón |
| **Visitas** | `POST /visitas`, `/mis-visitas`, `/service/{id}`, `/{id}/confirmar`, `DELETE /{id}` | Agenda de visitas a un salón |
| **Reservas** | `POST /reservas`, `/mis-reservas`, `/service/{id}`, `/desde-visita/{id}`, `/{id}/confirmar`, `PUT /{id}/financiero`, `DELETE /{id}` | Reservas, incluyendo conversión de una visita en reserva y su estado financiero |
| **Pagos** | `POST /pagos`, `PUT /pagos/{id}`, `GET /pagos/reserva/{id}` | Registro de pagos asociados a una reserva |
| **Resenas** | `POST /resenas`, `GET /resenas/service/{id}` | Reseñas y calificación de servicios |
| **Geography** | `GET /departamentos`, `/departamentos/{id}/barrios` | Departamentos y barrios de Uruguay para direcciones |
| **Event categories / Tags** | `GET /event-categories`, `GET /tags` | Catálogos de categorías y etiquetas |

## 🔒 Seguridad

- Autenticación JWT (HS256), con validación de issuer, audience y expiración
- Contraseñas hasheadas con BCrypt
- **Rate limiting**: 120 requests/minuto por IP a nivel global, y una política más estricta de 10 requests/minuto para endpoints de autenticación
- CORS restringido a los orígenes conocidos del frontend
- Middleware global de manejo de excepciones

## 🧪 Testing

No hay proyecto de tests en la solución (`BookIt-backend.sln` solo incluye `BookIt.API` y una herramienta de migraciones). Los 94 casos de prueba del proyecto (CP-01 a CP-94) están documentados en una planilla Excel, no como una suite automatizada — ver detalle en el [README del frontend](https://github.com/MatiOreiro/BookIt-frontend).

## ⚙️ Cómo correrlo localmente

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

## 👤 Autores

Matías Oreiro — [LinkedIn](https://www.linkedin.com/in/matiasoreiro/)
Matias Pietrafesa — [LinkedIn](https://www.linkedin.com/in/matias-pietrafesa-47084b321/)
