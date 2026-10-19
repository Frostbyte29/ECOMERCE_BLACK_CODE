# SOLID en Black Code — dónde está cada principio

> Cada dueño de módulo agrega las filas de su módulo en su PR. Esta tabla es lo que se muestra en la sustentación.

| Principio | Archivo | Qué demuestra | Autor |
|---|---|---|---|
| DIP | `BlackCode.Application/Abstractions/IAppDbContext.cs` + `BlackCode.Infrastructure/Persistence/AppDbContext.cs` | Application depende de una interfaz; solo Infrastructure sabe que la base es MySQL | Equipo |
| DIP | `BlackCode.Infrastructure/DependencyInjection.cs` + `BlackCode.Web/Program.cs` | Raíz de composición: el único lugar donde se conectan interfaces con implementaciones | Equipo |
| SRP | `BlackCode.Infrastructure/Security/AspNetContrasenaService.cs` | Hashear contraseñas es una responsabilidad separada del login | Equipo |
| SRP | `BlackCode.Infrastructure/Persistence/Configurations/*.cs` | Una clase de configuración por tabla | Equipo |
| OCP | `BlackCode.Domain/Pedidos/TransicionesPedido.cs` | Agregar un estado de pedido = agregar una entrada, sin tocar la lógica | Equipo |
| ISP | `BlackCode.Application/Abstractions/IUsuarioActual.cs` | Interfaz chica: solo lo que los servicios necesitan saber del usuario | Equipo |
| SRP | `BlackCode.Application/Features/Autenticacion/AutenticacionService.cs` y `Features/Usuarios/GestionUsuariosService.cs` | Autenticar y administrar usuarios son dos razones distintas para cambiar: dos servicios | Equipo |
| SRP | `BlackCode.Domain/Usuarios/Usuario.cs` (`RegistrarIntentoFallido`, `EstaBloqueado`) | La regla del bloqueo vive en la entidad y se prueba sin base de datos | Equipo |
| SRP | `BlackCode.Web/Security/CookieSesion.cs` | Armar la cookie es trabajo de Web; los servicios de Application no conocen `HttpContext` | Equipo |
| DIP | `AutenticacionService` depende de `IAppDbContext`, `IContrasenaService` y `TimeProvider` | Ninguna clase concreta de Infrastructure; el reloj se inyecta y los tests son deterministas | Equipo |
| ISP | `GestionUsuariosService` usa `IUsuarioActual` | Solo pide quién actúa y si es admin | Equipo |
| SRP | `BlackCode.Web/ViewComponents/MenuCuentaViewComponent.cs` | Renderizar el estado de sesión en el header es responsabilidad de Web; lee claims, no inyecta servicios de datos | Carlos |
| | | | |
