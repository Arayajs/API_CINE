# Cinema API

API REST en **ASP.NET Core 8** para gestionar cines, películas, funciones y la venta de entradas, con autenticación JWT y roles.

## Funcionalidades

- Registro e inicio de sesión con **JWT** y contraseñas con hash **BCrypt**.
- CRUD de cines, salas, películas y funciones. Solo el rol `Administrator` puede crear, editar o eliminar.
- Consulta de asientos disponibles por función.
- Órdenes de compra: crear, pagar, cancelar y consultar.
- Emisión de boletos con código único, validación y uso en puerta (solo `Administrator`) y reenvío por correo (MailKit).
- Registro de auditoría de operaciones.
- Documentación interactiva con **Swagger** (con soporte para token Bearer).

## Stack

ASP.NET Core 8 · Entity Framework Core 9 · SQL Server · AutoMapper · JWT Bearer · BCrypt.Net · MailKit · Swashbuckle

## Estructura

```
API_CINE/
├── Controllers/        # Endpoints HTTP
├── Services/           # Lógica de negocio (Interfaces + Implementations)
├── Repositories/       # Acceso a datos con Unit of Work
├── Models/Domain/      # Entidades
├── Models/DTOs/        # Contratos de entrada y salida
├── Data/               # DbContext
└── Migrations/         # Migraciones de EF Core
```

## Endpoints principales

| Recurso | Rutas |
|---|---|
| Auth | `POST /api/auth/register`, `POST /api/auth/login` |
| Cinemas | `GET /api/cinemas`, `/active`, `/{id}`, `/{id}/halls`, `/{id}/movies`, `/{id}/screenings` · `POST`, `PUT`, `DELETE` (admin) |
| Movies | `GET /api/movies`, `/active`, `/{id}`, `/{id}/screenings` · `POST`, `PUT`, `DELETE` (admin) |
| Orders 🔒 | `GET /api/orders`, `/{id}`, `/{id}/tickets` · `POST /api/orders` · `POST /{id}/process-payment` · `POST /{id}/cancel` · `GET /all` (admin) |
| Tickets 🔒 | `GET /api/tickets`, `/{id}`, `/code/{code}` · `GET /validate/{code}` y `POST /use/{code}` (admin) · `POST /{id}/resend-email` |

🔒 = requiere token. La lista completa está en Swagger.

## Cómo ejecutarlo

Requisitos: [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) y una instancia de SQL Server.

**1. Configura los secretos.** No guardes credenciales en `appsettings.json`. Usa [User Secrets](https://learn.microsoft.com/aspnet/core/security/app-secrets) en desarrollo:

```bash
cd API_CINE
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost;Database=DbCinema;Trusted_Connection=True;TrustServerCertificate=True"
dotnet user-secrets set "Jwt:Key" "una-clave-aleatoria-de-al-menos-32-caracteres"
```

Para el envío de correos, configura también la sección `EmailSettings` de la misma forma.

**2. Ejecuta.** En entorno `Development` las migraciones se aplican solas al iniciar:

```bash
dotnet run
```

**3. Abre Swagger** en `https://localhost:<puerto>/swagger`. Haz login, copia el token y pégalo en **Authorize** con el formato `Bearer {token}`.

## Qué demuestra este proyecto

- Diseño de una API REST por capas (controladores, servicios, repositorios, Unit of Work).
- Autenticación y autorización basadas en JWT y roles.
- Modelado relacional con EF Core y migraciones.
- Separación entre entidades de dominio y DTOs con AutoMapper.
