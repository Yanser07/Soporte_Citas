# Base de datos

La aplicación usa **SQL Server**. La API crea la base de datos `SoporteCitasDb`, sus tablas
(mediante migraciones de Entity Framework) y el usuario administrador al iniciar, así que
solo necesitas un SQL Server disponible y la cadena de conexión correcta.

- `schema.sql`: estructura de las tablas generada desde las migraciones, solo como
  referencia. No incluye el administrador ni el horario inicial: los crea la API al arrancar.

Los datos (usuarios, citas) **no** se guardan en este repositorio. Cada instalación empieza
con la base vacía.

Para regenerar `schema.sql` tras agregar una migración, desde la raíz del repositorio:

```bash
dotnet ef migrations script --idempotent --project backend/SoporteCitas.Api --output database/schema.sql
```