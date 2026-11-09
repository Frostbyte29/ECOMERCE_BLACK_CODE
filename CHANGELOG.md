# Changelog

Formato: [Keep a Changelog](https://keepachangelog.com/es-ES/1.1.0/) · Versionado: [SemVer](https://semver.org/lang/es/)

## [Sin publicar]

### RFC-2026-003 — Yape y Plin

#### Agregado
- Yape y Plin como implementaciones independientes de `IMetodoPago`, registradas mediante inyección de dependencias y disponibles en el selector del checkout.
- QR de cada billetera en el checkout, con archivos públicos `wwwroot/img/pagos/yape-qr.png` y `plin-qr.png`. Si falta una imagen, se muestra un aviso. Los pagos continúan siendo simulados.
- Pruebas de resolución de los cuatro métodos, rechazo, códigos inválidos, persistencia del método elegido, reversión transaccional y lectura de pedidos históricos. SQLite se usa únicamente como base temporal de pruebas; la tienda conserva MySQL.

#### Corregido
- Cada pago nuevo guarda el método seleccionado: Tarjeta = 2, Transferencia = 3, Yape = 4 y Plin = 5. `Simulado = 1` conserva el significado de los registros anteriores.
- Ajuste común del contrato: `IMetodoPago.Metodo` identifica la estrategia y `PagoService` lo transporta en `ResultadoPagoDto`. `PedidoService` deja de guardar un valor fijo. No se añaden condiciones por billetera al coordinador ni migraciones.

### Iteración 2 — flujo de compras

#### Agregado
- Filtro de material en el catálogo: acepta `material` por GET y se combina con categoría, color, talla, precio, búsqueda y orden sin modificar `CatalogoService` (OCP — `FiltroMaterial` como nueva clase independiente). El detalle del producto muestra el nombre y la descripción del material.
- Flujo completo de compra: carrito para invitados y usuarios registrados, checkout con formulario de envío, selección de método de pago y confirmación con descuento de stock.
- Precarga del formulario de checkout con los datos del último envío del usuario; si no hay pedidos previos usa el teléfono guardado en la cuenta.
- Pruebas del servicio de pedidos: precarga del envío más reciente, selección del pedido más reciente por fecha y fallback al teléfono de la cuenta.

#### Corregido
- El carrito de un invitado se conserva y se fusiona al iniciar sesión.
- El administrador no puede acceder al carrito, al checkout ni a la vista de pedidos de la tienda.
- Teléfono de envío validado (exactamente 9 dígitos); el botón de confirmación no queda bloqueado si el cliente corrige un error de validación.
- Precio del producto analizado sin depender de la cultura del sistema operativo anfitrión.
- Estilos de las páginas de compra aislados en sus propias vistas, sin afectar el panel.
- Menú de la tienda enlazado con catálogo, carrito y pedidos.
- Textos de simulación de pago eliminados de las vistas del flujo de compra.
- Restauración de cookie de sesión, perfiles de ejecución y navegación del panel tras la integración de ramas.
- Detalles visuales de la tienda y el panel corregidos.

### Pendiente para la iteración 2
- Pedido pendiente de pago con expiración automática.
- Pago contra entrega.
- Número de seguimiento del pedido.
- Libreta de direcciones guardadas.
- Historial de stock.
- Rediseño del Home, del checkout por pasos y de Mis pedidos.
- Panel de administración con inicio y dashboard.
- Páginas Nosotros, Colecciones y Favoritos.
- Filtros y cantidades del carrito sin botones intermedios.
- Buscador con sugerencias de texto (autocompletado).
- Conteos de resultados junto a cada opción de filtro.
- Gestión de categorías, materiales y colores desde el panel.
- Imagen del producto por subida de archivo en lugar de la lista fija.
- Mensajes de éxito y error visibles, botón que se deshabilita al enviar e identificador URL generado desde el nombre en el formulario de producto.
- Preguntas frecuentes.
- Recuperación de contraseña por correo electrónico.
- Notificación al administrador cuando entra un pedido nuevo.

---

### Iteración 1 — base del proyecto

#### Agregado
- Adaptación visual a las capturas: portada horizontal, colecciones, pie oscuro, acceso con foto a la derecha y navegación lateral administrativa.
- Fotografías editoriales y emblemas originales de marca con variantes para fondos claros y oscuros.
- Prototipo independiente de los cinco módulos: cuentas, catálogo, carrito, pedidos/pago simulado y administración.
- Carrito de invitados y fusión al iniciar sesión; control de stock y operaciones de compra transaccionales.
- Interfaz en español con navbar fijo, submenús, filtros a la izquierda y vistas móviles.
- Ocho ilustraciones locales y datos iniciales para anillos, cadenas y pulseras.
- Pruebas del carrito, verificación en navegador y concurrencia sobre MySQL.
- Guías de Visual Studio/F5, MySQL Workbench, alcance y sustentación SOLID.
- Estructura en 4 capas (Web, Application, Domain, Infrastructure) + proyecto de tests.
- Modelo de datos inicial y migración `InitialSchema`.
- Datos de demo: 6 anillos, categorías, usuario admin y cliente de prueba.
- Registro, inicio y cierre de sesión de clientes (`/Cuenta/Login`, `/Cuenta/Registro`) con "mantener sesión".
- Bloqueo de la cuenta por 15 minutos tras 5 intentos fallidos.
- Acceso de administrador separado (`/Admin/Login`) con la misma cookie de sesión.
- Panel → Usuarios: búsqueda y filtros, activar o desactivar y desbloquear.
- La sesión de un usuario desactivado se invalida en la siguiente petición.
- Layout global de la tienda: announcement bar, header con navegación, footer de 4 columnas, sin Bootstrap.
- CSS base de la tienda (`blackcode.css`) con variables en `:root` y diseño responsivo.
- ViewComponent `MenuCuenta`: sin sesión muestra "Iniciar sesión / Crear cuenta"; con sesión muestra nombre y opciones.
- Corrección en validación de sesión: petición cancelada por el cliente ya no invalida la cookie.

#### Eliminado
- Enlace de recuperación de contraseña del formulario de login (se implementará por correo más adelante).

## [0.1.0]
