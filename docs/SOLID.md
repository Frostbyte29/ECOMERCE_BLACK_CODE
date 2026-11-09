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
| SRP | `BlackCode.Application/Features/Autenticacion/AutenticacionService.cs` y `Features/Clientes/GestionClientesService.cs` | Autenticar y administrar clientes son dos razones distintas para cambiar: dos servicios | Carlos |
| SRP | `BlackCode.Web/Security/CookieSesion.cs` | Armar la cookie es trabajo de Web; los servicios de Application no conocen `HttpContext` | Equipo |
| DIP | `AutenticacionService` depende de `IAppDbContext`, `IContrasenaService` y `TimeProvider` | Ninguna clase concreta de Infrastructure; el reloj se inyecta y los tests son deterministas | Carlos |
| ISP | `GestionClientesService` usa `IUsuarioActual` | Solo pide quién actúa y si es admin | Carlos |
| SRP | `BlackCode.Web/ViewComponents/MenuCuentaViewComponent.cs` | Renderizar el estado de sesión en el header es responsabilidad de Web; lee claims, no inyecta servicios de datos | Carlos |
| SRP | `BlackCode.Application/Features/Carrito/CarritoService.cs` y `CalculoCarritoService.cs` | Gestionar artículos está separado de calcular importes, descuentos y envío | Angelo |
| OCP | `BlackCode.Application/Features/Carrito/CarritoDtos.cs` y `CalculoCarritoService.cs` | IReglaDescuento admite nuevas implementaciones; la calculadora recibe una colección y no cambia al añadir una regla | Angelo |
| DIP | `BlackCode.Application/Features/Carrito/CarritoService.cs` | Depende de IAppDbContext e ICalculoCarritoService; no conoce MySQL ni AppDbContext | Angelo |
| DIP | `BlackCode.Web/Controllers/CarritoController.cs` | Recibe ICarritoService por constructor y mapea el DTO a un ViewModel; no consulta tablas | Angelo |
| SRP / DIP | `BlackCode.Application/Features/Pedidos/PedidoService.cs` | Coordina la compra con IStockGateway e IPagoService; el procesamiento del pago está separado | Angelo |
| OCP | `BlackCode.Web/Program.cs` — `AddAuthorization` | La restricción se define como política declarativa en la raíz de composición; la lógica de las acciones y de los servicios no cambia; los controladores solo reciben el atributo | Carlos |
| SRP | `BlackCode.Web/Models/TiendaViewModels.cs` — `CheckoutViewModel` | Una sola responsabilidad: ser el contrato de datos del formulario de checkout, con sus reglas de validación declaradas en DataAnnotations | Carlos |
| SRP | `BlackCode.Web/ViewModels/MustBeTrueAttribute.cs` | La regla "el campo bool debe ser true" es una responsabilidad separada del ViewModel; el atributo encapsula la validación sin mezclarla con la estructura del formulario | Carlos |
| OCP | `BlackCode.Application/Features/Catalogo/FiltrosProducto.cs` | FiltroMaterial se agrega como clase nueva que implementa IFiltroProducto; CatalogoService no se modifica para incorporar el criterio | Carlos |

## Cómo demostrar los tres principios en el carrito

1. **SRP:** compara CarritoService.AgregarAsync con CalculoCarritoService.CalcularAsync.
   Una clase modifica artículos; otra aplica las reglas de importes.
2. **OCP:** la calculadora utiliza IEnumerable<IReglaDescuento>. SinDescuento es la política activa.
   DescuentoPorCantidad muestra otra implementación y tiene una prueba que verifica su integración.
   Para activarla se cambia el registro en DependencyInjection, sin modificar la calculadora.
   No hay descuentos comerciales activos por defecto.
3. **DIP:** revisa el constructor de CarritoService y el registro de servicios.
   Application depende de abstracciones; Infrastructure implementa el acceso concreto.
   IAppDbContext sigue exponiendo tipos de EF Core: el diseño abstrae MySQL, no todo el ORM.

La tabla original menciona las transiciones de pedido como ejemplo OCP. Ese ejemplo mantiene
el algoritmo de consulta al cambiar los datos, pero añadir estados todavía requiere editar
el enum y el diccionario. La extensión por IReglaDescuento es el ejemplo principal del proyecto.

## Matriz final de los cinco módulos

| Módulo | SRP: responsabilidades separadas | OCP: punto de extensión | DIP: dependencias mediante contratos |
|---|---|---|---|
| Autenticación | AutenticacionService autentica (PortalAcceso.Tienda / Panel); GestionClientesService administra clientes. CookieSesion arma la cookie. IContrasenaService hashea. | Sin punto de extensión propio: hay un solo método de acceso por portal. | AutenticacionService recibe IAppDbContext, IContrasenaService y TimeProvider |
| Catálogo | CatalogoService consulta; CatalogoController mapea; las vistas presentan ViewModels | IFiltroProducto tiene implementaciones de texto, categoría, color, material, precio y talla. Cada criterio nuevo es una clase independiente; CatalogoService no cambia | CatalogoService recibe IAppDbContext e IEnumerable<IFiltroProducto>; el controlador recibe ICatalogoService |
| Carrito | CarritoService modifica artículos y fusiona; CalculoCarritoService calcula importes | IReglaDescuento permite SinDescuento y DescuentoPorCantidad sin cambiar la calculadora | CarritoService recibe IAppDbContext e ICalculoCarritoService; la calculadora recibe reglas mediante interfaz |
| Pedidos y pagos | PedidoService coordina la compra; PagoService procesa mediante estrategias; NotificacionPedidoService prepara la confirmación en pantalla | IMetodoPago permite PagoTarjetaSimulado, PagoTransferenciaSimulado, PagoYapeSimulado y PagoPlinSimulado. La selección usa el código de la implementación, sin condicionales por método | PedidoService depende de IAppDbContext, ICarritoService, IStockGateway, IPagoService e IUsuarioActual |
| Administración | AdministracionService gestiona productos; InventarioService modifica variantes y stock; PedidoService gestiona pedidos; ReporteService prepara exportaciones | IExportadorReporte permite ExportadorCsv y ExportadorJson; una prueba incorpora un exportador nuevo sin cambiar ReporteService | Los servicios reciben IAppDbContext, IUsuarioActual, IInventarioService e interfaces de exportación; AdminController recibe servicios |

### Ubicación de los ejemplos

Todos los servicios y contratos están en `BlackCode.Application/Features/<Módulo>`.
Las implementaciones de persistencia siguen en `BlackCode.Infrastructure`, y la conexión de interfaces
con implementaciones está en `BlackCode.Application/DependencyInjection.cs` y
`BlackCode.Infrastructure/DependencyInjection.cs`.

Se conserva **IAppDbContext**, la abstracción que exige el README. Cumple el papel de acceso
a datos que el cuadro ilustraba con RepositorioUsuarios o RepositorioProductos; no se han
introducido repositorios duplicados ni controladores que accedan directamente al contexto.

### Pruebas de la extensión

- `BlackCode.Tests/Solid/ExtensibilidadTests.cs`: criterio nuevo del catálogo,
  método adicional de pago, exportador adicional, permisos e inventario.
- `BlackCode.Tests/Carrito/CarritoServiceTests.cs`: regla nueva de descuento, cálculo de envío,
  aislamiento de invitados, fusión y límites de cantidad.

En autenticación solo se ofrece correo/contraseña en la interfaz actual.
Las cuatro opciones de pago son simuladas. Las notificaciones son mensajes en la pantalla.

### Presentación de la marca

| Principio | Archivo | Qué demuestra | Autor |
|---|---|---|---|
| SRP | `BlackCode.Web/Views/Shared/_Marca.cshtml` | Centraliza únicamente la representación de la marca; las variantes de contraste se resuelven en `wwwroot/css/marca.css`, sin añadir reglas de negocio a las vistas. | Angelo |

La incorporación de fotografías y logos no cambia servicios, contratos ni persistencia.
OCP y DIP conservan las implementaciones y pruebas descritas en las secciones anteriores.

### RFC-2026-003 — Yape y Plin

| Principio | Archivo | Qué demuestra | Autor |
|---|---|---|---|
| SRP | `Features/Pedidos/PagoYapeSimulado.cs` y `PagoPlinSimulado.cs` en Application | Cada clase simula únicamente el pago de su billetera. La presentación del QR pertenece al checkout de Web. | Angelo |
| OCP | `IMetodoPago`, `PagoYapeSimulado`, `PagoPlinSimulado` y `Application/DependencyInjection.cs` | Los métodos nuevos se agregan como clases registradas. La selección genérica de `PagoService` no incorpora casos para Yape ni Plin. | Angelo |
| DIP | `PedidoService` y `PagoService` | El pedido recibe `IPagoService` y el coordinador recibe `IEnumerable<IMetodoPago>`. El valor persistido procede de la estrategia seleccionada. | Angelo |

El ajuste común para guardar el método introduce `IMetodoPago.Metodo` y transporta
ese valor en `ResultadoPagoDto`. Por eso cambia una vez `PagoService`: agrega el
valor al resultado de cualquier estrategia, sin reconocer billeteras concretas.
Agregar las dos clases y sus registros no requiere otra modificación del coordinador.

El checkout conserva su lista dinámica desde `IPagoService.Metodos()`. Web asocia
las imágenes a los códigos registrados y envía esa información mediante
`MetodoPagoViewModel`, sin pasar entidades a la vista.

`pagos.metodo` permanece como `int`. El valor 1 conserva los pagos históricos y
los valores 2 a 5 identifican los métodos nuevos. No se modifica el esquema.
La prueba `Modelo_mysql_conserva_el_esquema_de_las_migraciones` verifica el modelo
contra el snapshot sin conectarse a una base de datos.

Pruebas: `BlackCode.Tests/Pedidos/PagoServiceTests.cs` y `CheckoutPagoTests.cs`.
Las compras completas usan SQLite temporal y un adaptador de stock de prueba.
Esto comprueba la coordinación y persistencia del checkout, no el SQL específico de MySQL.
