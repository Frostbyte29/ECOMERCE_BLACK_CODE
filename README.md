# BLACK CODE

Ecommerce de accesorios para hombre (anillos, cadenas y pulseras). Proyecto del curso Evolución de Software, UPN.

- [Abrir con F5 y conectar MySQL Workbench](docs/INICIO.md)
- [Principios SOLID aplicados](docs/SOLID.md)

## Tecnología

- ASP.NET Core MVC con .NET 10, en Visual Studio 2026
- Entity Framework Core 10 con MySQL 8.4 (paquete `MySql.EntityFrameworkCore`)
- Autenticación con cookie y `PasswordHasher` de ASP.NET Core
- Pruebas con xUnit

## Arquitectura

Cuatro capas más un proyecto de pruebas. Las flechas indican quién referencia a quién.

| Proyecto                   | Responsabilidad                                       | Referencia a                |
| -------------------------- | ----------------------------------------------------- | --------------------------- |
| `BlackCode.Domain`         | Entidades y reglas puras del negocio                  | nadie                       |
| `BlackCode.Application`    | Servicios, casos de uso y contratos (interfaces)      | Domain                      |
| `BlackCode.Infrastructure` | EF Core, MySQL, hash de contraseñas, datos de demo    | Application                 |
| `BlackCode.Web`            | Controladores, vistas Razor, ViewModels, `Program.cs` | Application, Infrastructure |
| `BlackCode.Tests`          | Pruebas unitarias y de integración                    | Domain, Application         |

Reglas que se respetan siempre:

1. Flujo: controlador → servicio de Application → `IAppDbContext`. Ningún controlador usa `AppDbContext` directamente.
2. Ninguna vista recibe una entidad: siempre un ViewModel.
3. Domain no depende de ningún otro proyecto. Application no sabe que la base es MySQL.
4. Controladores delgados: sin lógica de negocio.
5. El mapeo es manual (sin AutoMapper) y el SQL va siempre parametrizado.
6. Fechas en UTC y reloj inyectado (`TimeProvider`).

La explicación de cómo se aplica cada principio SOLID está en [`docs/SOLID.md`](docs/SOLID.md).

## Primeros pasos

### 1. Clonar

```
git clone https://github.com/Frostbyte29/ECOMERCE_BLACK_CODE.git
cd ECOMERCE_BLACK_CODE/BlackCode
```

Abre la solución `BlackCode.slnx` con Visual Studio 2026 (carga de trabajo "Desarrollo de ASP.NET y web").

### 2. Crear la base de datos en MySQL

Ejecuta esto una vez en MySQL Workbench o DBeaver. Elige un nombre de base y usuario propios y úsalos también en el paso 3.

> Los valores concretos (`[NOMBRE_BASE]`, `[USUARIO]`, `[CLAVE]`) no están en el repositorio: los define cada integrante en sus secretos de usuario.

```sql
CREATE DATABASE IF NOT EXISTS [NOMBRE_BASE] CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci;
CREATE USER IF NOT EXISTS '[USUARIO]'@'localhost' IDENTIFIED BY '[CLAVE]';
GRANT ALL PRIVILEGES ON [NOMBRE_BASE].* TO '[USUARIO]'@'localhost';
FLUSH PRIVILEGES;
```

### 3. Configurar los secretos de usuario

En el Explorador de soluciones: clic derecho en `BlackCode.Web` → **Administrar secretos de usuario**. Pega esto con los valores elegidos en el paso 2:

> Fuente de los valores: user-secrets de cada integrante, no commitados. Los valores entre corchetes deben coincidir con lo que creaste en el paso anterior.

```json
{
  "ConnectionStrings": {
    "Default": "Server=localhost;Port=3306;Database=[NOMBRE_BASE];Uid=[USUARIO];Pwd=[CLAVE];AllowPublicKeyRetrieval=True;"
  },
  "Seed": {
    "AdminPassword": "clave para admin@blackcode.pe",
    "ClientePassword": "clave para cliente@demo.pe"
  }
}
```

Guarda con Ctrl+S. Este archivo vive en tu computadora, fuera del repositorio. **Nunca subas claves.**

### 4. Ejecutar

Establece `BlackCode.Web` como proyecto de inicio y pulsa F5 con el perfil `https`. En Desarrollo, la aplicación aplica las migraciones y carga los datos de demo (categorías, productos, usuarios) al arrancar.

### 5. Cuentas de prueba

| Cuenta | Rol | Acceso |
| ------ | --- | ------ |
| `cliente@demo.pe` | Cliente | Tienda: catálogo, carrito, checkout, mis pedidos |
| `admin@blackcode.pe` | Administrador | Panel: productos, variantes, pedidos, clientes, reportes |

- Registro de clientes en `/Cuenta/Registro`. El rol siempre es Cliente; nunca viene del formulario.
- Un administrador puede activar o desactivar cuentas de clientes desde **Panel → Clientes → Editar**.
- El administrador no puede usar la tienda: carrito y checkout están restringidos a clientes.
- Si un administrador desactiva a un usuario, su sesión abierta se cierra en la siguiente petición.

### 6. Pruebas

Menú **Prueba → Explorador de pruebas → Ejecutar todas**, o `dotnet test` en la raíz del repositorio.

## Flujo de trabajo

- Ramas: `main` (estable) ← `develop` (integración) ← una rama por persona o tarea.
- **Nadie hace push directo a `develop` ni a `main`.** Todo entra por Pull Request con la plantilla llena.
- Cada integrante commitea desde su propia cuenta. Verifica que tu correo de Git coincida con el de tu cuenta de GitHub:
  ```
  git config --global user.email "tu-correo-de-github"
  ```
- Commits pequeños y frecuentes, con mensajes en español (`feat:`, `fix:`, `docs:`, `refactor:`, `test:`).
- Si estás bloqueado más de 30 minutos, avisa en el grupo.
- **Migraciones de base de datos.** Si un cambio necesita una columna o tabla nueva: (1) modifica la entidad y su configuración en `Infrastructure/Persistence/Configurations`; (2) avisa al líder del equipo, que corre `Add-Migration` y abre el Pull Request con la migración. Reglas que se respetan siempre:
  1. Una sola migración por Pull Request, con nombre descriptivo (por ejemplo `AgregarEstadoEnvio`).
  2. Nunca edites ni borres una migración que ya esté en `develop`.
  3. Si el merge choca en `AppDbContextModelSnapshot.cs`, ejecuta `Remove-Migration`, actualiza desde `develop` y vuelve a generarla.

## Cómo agregar una funcionalidad

1. **Domain:** si necesita una entidad o campo nuevo, agrégalo con su configuración en `Infrastructure/Persistence/Configurations`.
2. **Application/Features/\<Funcionalidad\>:** interfaz del servicio, su implementación y los DTO de entrada y salida. El servicio depende de interfaces (`IAppDbContext`, `IUsuarioActual`, `TimeProvider`), nunca de clases concretas de Infrastructure.
3. Registra el servicio en `Application/DependencyInjection.cs`.
4. **Web:** el controlador recibe el servicio por constructor, convierte DTO ↔ ViewModel y devuelve la vista.
5. Vista Razor con su ViewModel, formularios con `asp-for` y `asp-validation-for`, y CSS propio de la pantalla.
6. Agrega una fila a `docs/SOLID.md` si el cambio muestra un principio.
7. Abre el Pull Request.

## Nomenclatura

El vocabulario del negocio va en español (`Producto`, `Pedido`, `Carrito`, `EstadoPedido`…). Los nombres técnicos o de patrón de programación van en inglés (`Controller`, `Service`, `ViewModel`, `Configuration`, `DbContext`, `Common`, `Abstractions`, `Persistence`, `Security`, `DomainException`…). Las tablas y columnas de la base de datos van en español, en minúsculas con guion bajo (`variantes_producto`, `creado_utc`).

## Documentos

- [`docs/SOLID.md`](docs/SOLID.md): dónde y cómo se aplica cada principio SOLID.
- [`CHANGELOG.md`](CHANGELOG.md): historial de cambios.
