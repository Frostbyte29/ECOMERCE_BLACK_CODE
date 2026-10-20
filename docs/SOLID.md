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
| SRP | `BlackCode.Application/Features/Carrito/CarritoService.cs` y `CalculoCarritoService.cs` | Gestionar artículos está separado de calcular importes, descuentos y envío | Angelo — prototipo asistido |
| OCP | `BlackCode.Application/Features/Carrito/CarritoDtos.cs` y `CalculoCarritoService.cs` | IReglaDescuento admite nuevas implementaciones; la calculadora recibe una colección y no cambia al añadir una regla | Angelo — prototipo asistido |
| DIP | `BlackCode.Application/Features/Carrito/CarritoService.cs` | Depende de IAppDbContext e ICalculoCarritoService; no conoce MySQL ni AppDbContext | Angelo — prototipo asistido |
| DIP | `BlackCode.Web/Controllers/CarritoController.cs` | Recibe ICarritoService por constructor y mapea el DTO a un ViewModel; no consulta tablas | Angelo — prototipo asistido |
| SRP / DIP | `BlackCode.Application/Features/Pedidos/PedidoService.cs` | Coordina la compra con IStockGateway e IPagoService; el procesamiento del pago está separado | Angelo — prototipo asistido |

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
el enum y el diccionario. La extensión por IReglaDescuento es el ejemplo principal del prototipo.

## Matriz final de los cinco módulos

| Módulo | SRP: responsabilidades separadas | OCP: punto de extensión | DIP: dependencias mediante contratos |
|---|---|---|---|
| Autenticación | RegistroValidator valida datos; ProveedorContrasena autentica; IAppDbContext separa el acceso persistente; la cookie se crea en CuentaController | IProveedorAutenticacion permite añadir proveedores sin cambiar CuentaService. ProveedorContrasena es el proveedor activo; la prueba incorpora otro proveedor sin cambiar el servicio | CuentaService recibe IAppDbContext, IContrasenaService, IRegistroValidator e IEnumerable<IProveedorAutenticacion> |
| Catálogo | CatalogoService consulta; CatalogoController mapea; las vistas presentan ViewModels | IFiltroProducto tiene implementaciones de texto, categoría, color, precio y talla. La prueba añade un criterio adicional sin modificar CatalogoService | CatalogoService recibe IAppDbContext e IEnumerable<IFiltroProducto>; el controlador recibe ICatalogoService |
| Carrito | CarritoService modifica artículos y fusiona; CalculoCarritoService calcula importes | IReglaDescuento permite SinDescuento y DescuentoPorCantidad sin cambiar la calculadora | CarritoService recibe IAppDbContext e ICalculoCarritoService; la calculadora recibe reglas mediante interfaz |
| Pedidos y pagos | PedidoService coordina la compra; PagoService procesa mediante estrategias; NotificacionPedidoService prepara la confirmación en pantalla | IMetodoPago permite PagoTarjetaSimulado y PagoTransferenciaSimulado. La selección usa el código de la implementación, sin condicionales por método | PedidoService depende de IAppDbContext, ICarritoService, IStockGateway, IPagoService e IUsuarioActual |
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

- `BlackCode.Tests/Solid/ExtensibilidadTests.cs`: proveedor adicional de autenticación, criterio nuevo del catálogo,
  método adicional de pago, exportador adicional, bloqueo de login, permisos e inventario.
- `BlackCode.Tests/Carrito/CarritoServiceTests.cs`: regla nueva de descuento, cálculo de envío,
  aislamiento de invitados, fusión y límites de cantidad.

En autenticación solo se ofrece correo/contraseña en la interfaz actual. La extensión está implementada
y probada; no se simula una integración con Google o un proveedor externo inexistente.
Las dos opciones de pago son simuladas. Las notificaciones son mensajes en la pantalla.

### Presentación de la marca

| Principio | Archivo | Qué demuestra | Autor |
|---|---|---|---|
| SRP | `BlackCode.Web/Views/Shared/_Marca.cshtml` | Centraliza únicamente la representación de la marca; las variantes de contraste se resuelven en `wwwroot/css/marca.css`, sin añadir reglas de negocio a las vistas. | Angelo |

La incorporación de fotografías y logos no cambia servicios, contratos ni persistencia.
OCP y DIP conservan las implementaciones y pruebas descritas en las secciones anteriores.
