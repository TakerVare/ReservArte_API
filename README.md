# ReservArte_API

Repositorio para la API de la aplicación de reservas de trabajo final de 2º de DAW.

## Descripción

Esta API backend está desarrollada en ASP.NET Core y proporciona los servicios necesarios para la gestión de reservas, clientes, empleados, servicios y pagos en una aplicación de reservas para un negocio de estética o similar.

## Tecnologías Utilizadas

- **ASP.NET Core**: Framework para el desarrollo de la API.
- **Entity Framework Core**: ORM para la interacción con la base de datos.
- **SQL Server**: Base de datos relacional.
- **Docker**: Contenedorización para facilitar el despliegue.
- **Swagger/OpenAPI**: Documentación interactiva de la API.

## Instalación y Ejecución

### Prerrequisitos

- Docker y Docker Compose instalados en tu sistema.

### Ejecutar con Docker (Entorno Completo)

1. Clona este repositorio:
   ```
   git clone <url-del-repositorio>
   cd ReservArte_API
   ```

2. Ejecuta Docker Compose para iniciar la base de datos y la API:
   ```
   docker-compose up --build -d
   ```

   Esto iniciará SQL Server y la API .NET en `http://localhost:5297`.

### Ejecutar Localmente (Sin Docker)

1. Asegúrate de tener .NET 8 SDK instalado.
2. Instala SQL Server localmente o usa una instancia existente.
3. Actualiza la cadena de conexión en `appsettings.json`.
4. Ejecuta las migraciones de Entity Framework:
   ```
   dotnet ef database update
   ```
5. Ejecuta la aplicación:
   ```
   dotnet run
   ```

## Documentación de la API

La documentación completa de la API está disponible a través de Swagger en:
- `http://localhost:5297/swagger/index.html`

### Endpoints Principales

- **Autenticación**: `/api/Auth`
- **Clientes**: `/api/Customer`
- **Empleados**: `/api/Employee`
- **Servicios**: `/api/Service`
- **Citas**: `/api/Appointment`
- **Pagos**: `/api/Payment`
- **Inventario**: `/api/Inventory`
- **Recordatorios**: `/api/Reminder`
- **Lista de Espera**: `/api/WaitingList`

## Credenciales de Prueba

Para testing, puedes usar las siguientes credenciales de usuarios de prueba:

| Rol          | Email                      | Contraseña   |
|--------------|----------------------------|--------------|
| Administrador| guille@svalero.com         | Admin1234!   |
| Empleada     | maria.garcia@reservarte.com| Maria123!    |
| Empleada     | laura.martinez@reservarte.com| Laura123! |
| Cliente      | ana.lopez@email.com        | Cliente123!  |
| Cliente      | carmen.rodriguez@email.com | Cliente123!  |
| Cliente      | isabel.sanchez@email.com   | Cliente123!  |

## Testing

El proyecto incluye una suite de pruebas unitarias en `ReservArte_API.Tests` con xUnit y Moq, centradas en el `AppointmentController` y sus DTOs.

Para ejecutar los tests:
```
cd ReservArte_API.Tests
dotnet test
```

Informe de cobertura interactivo: [https://gasvm.github.io/CoverageReport/](https://gasvm.github.io/CoverageReport/)