# Soporte Citas

Aplicación web para agendar citas de soporte técnico. Los clientes solicitan citas y un único
administrador las acepta, rechaza o completa, con avisos por correo en ambos sentidos.

## Estructura

```
backend/    API REST en ASP.NET Core (.NET 10) con Entity Framework Core y SQL Server
frontend/   Interfaz en Vue 3, Pinia, Vue Router y Tailwind CSS
database/   Script SQL con la estructura de las tablas y notas
```

## Funcionalidades

- Registro, inicio de sesión (JWT con roles) y recuperación de contraseña por correo.
- Calendario con horarios disponibles, sin permitir dos citas a la misma hora.
- Modalidad de atención: remoto, en el local o a domicilio.
- Panel de administrador: aceptar o rechazar con motivo, completar con notas, búsqueda,
  filtros y paginación.
- Gestión del horario de atención y de fechas bloqueadas.
- Correos automáticos al administrador y a los clientes.
- Seguridad: BCrypt, límite de peticiones, bloqueo temporal por intentos fallidos.

## Tecnologías

C# · ASP.NET Core · Entity Framework Core · SQL Server · JWT · MailKit · Vue 3 · Pinia ·
Vue Router · Axios · Tailwind CSS · Vite

## Cómo ejecutarlo

Requisitos: .NET SDK 10, Node.js 20 o superior, SQL Server (Express o LocalDB) y Git.

### 1. Backend

```bash
cd backend/SoporteCitas.Api
```

Guarda los secretos (no se suben al repositorio):

```bash
dotnet user-secrets set "Jwt:Key" "una-clave-aleatoria-de-al-menos-32-caracteres"
dotnet user-secrets set "Admin:Nombre" "Administrador"
dotnet user-secrets set "Admin:Correo" "admin@ejemplo.com"
dotnet user-secrets set "Admin:Password" "UnaClaveSegura123!"
dotnet user-secrets set "Email:Usuario" "cuenta-que-envia@gmail.com"
dotnet user-secrets set "Email:Password" "contraseña-de-aplicacion-de-gmail"
```

Si tu SQL Server no es `localhost\SQLEXPRESS`, define también la conexión:

```bash
dotnet user-secrets set "ConnectionStrings:Default" "Server=...;Database=SoporteCitasDb;Trusted_Connection=True;TrustServerCertificate=True;"
```

Inicia la API (crea la base de datos, las tablas y el administrador automáticamente):

```bash
dotnet run
```

Anota el puerto HTTPS que muestra la consola. En desarrollo, la documentación está en
`https://localhost:<puerto>/swagger`.

### 2. Frontend

```bash
cd frontend
npm install
copy .env.example .env
```

Edita `.env` y pon el puerto de tu API, por ejemplo `VITE_API_URL=https://localhost:7123/api`. Luego:

```bash
npm run dev
```

Abre `http://localhost:5173` e inicia sesión con el correo y la contraseña del administrador
que guardaste en los secretos.