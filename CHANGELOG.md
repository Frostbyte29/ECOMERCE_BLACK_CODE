# Changelog

Formato: [Keep a Changelog](https://keepachangelog.com/es-ES/1.1.0/) · Versionado: [SemVer](https://semver.org/lang/es/)

## [Sin publicar]

### Header: barra de búsqueda e insignia de carrito
- Barra de búsqueda en el header: se abre con el icono de lupa, se cierra con Esc o pulsando fuera y envía al catálogo con `?Buscar=`.
- Insignia numérica en el icono del carrito: muestra las unidades totales; se oculta cuando el carrito está vacío (`InsigniaCarritoViewComponent`, SRP + DIP: depende de `ICarritoService`).
- Iconos de búsqueda y carrito reemplazados por SVG inline accesibles.
- Dos pruebas nuevas de `ContarUnidadesAsync` en `CarritoServiceTests`.

### Rediseño del detalle de producto
- Página de detalle con breadcrumb usando la categoría real, especificaciones de material, selector de talla con radio buttons, stepper de cantidad con indicador de stock en tiempo real y galería de miniaturas cuando hay más de una imagen.
- Error de cantidad en detalle redirige de vuelta al producto (PRG), conservando la variante y la cantidad seleccionadas en TempData.
- Dos pruebas nuevas de límite en `CarritoServiceTests`: cantidad exactamente igual al stock no lanza excepción; cantidades fuera de rango reportan el fragmento de mensaje correcto.

### Realismo de textos e interfaz de producto
- Mensajes de cara al usuario saneados: sin referencias a entorno de pruebas ni a cobros ficticios en el checkout.
- Términos y Condiciones reescritos como tienda real de joyería en Perú: aceptación, cuenta, productos y precios en soles, métodos de pago, envíos a todo el país (2–4 días hábiles), cambios y devoluciones (7 días calendario por talla o defecto), propiedad intelectual, limitación de responsabilidad, ley aplicable. Sin datos inventados.
- Política de Privacidad reescrita: qué datos se recopilan, finalidad, terceros (solo pago y envío), hash de contraseñas, cookies de sesión y derechos del usuario.
- "Forzar rechazo" oculto por defecto; disponible solo con `?pruebas=1` en entorno Development.

### Política comercial desde base de datos
- `IPoliticaComercialService` lee las claves de `configuracion_tienda` y las expone en un DTO tipado: plazos de envío y devoluciones, costo y umbral de envío gratis, mensajes de barra y redes sociales centralizados.
- Seeder idempotente `EnsureConfigDefaultsAsync`: añade las claves faltantes a bases de datos existentes sin sobreescribir valores ya modificados.
- Consumidores: detalle de producto, checkout, detalle de pedido, Términos y Condiciones y `_Layout.cshtml` (barra de anuncio y enlaces de redes sociales).
- Cuatro pruebas nuevas en `PoliticaComercialServiceTests`: valores almacenados, valores por defecto, URL vacía devuelve null, mensajes de barra excluye vacíos y preserva orden.

### Vocabulario de estados de pedido
- `TiendaMapper.Estado` cubre los seis valores de `EstadoPedido` con texto visible para el cliente: Pendiente de pago, Pago confirmado, Preparando tu pedido, En camino, Entregado y Cancelado.
- Prueba que itera todos los valores del enum y falla si alguno devuelve el nombre crudo.

---

## [0.2.0] - 2026-11-10

### Catálogo fotográfico y edición de imágenes
- Colección de 19 anillos con fotografías propias, nombres y precios sugeridos, descripción, color, material y cinco tallas con stock inicial. Se incorpora una sola vez en Development; los seis anillos SVG originales se retiran a borrador; cadena y pulsera de eslabones siguen publicados.
- Selección de JPG, PNG o WebP hasta 5 MB al crear o editar, con vista previa en el navegador. Validación en cliente y servidor; almacenamiento detrás de una interfaz (`IImagenProductoService`); limpieza de la carga nueva si el producto no se guarda (compensación transaccional).
- Tarjetas de Inicio y Catálogo con imagen a todo el ancho, nombre y precio agrupados y precio destacado en soles, también en móvil.
- Guía de fotografías y datos iniciales en `docs/CATALOGO-FOTOGRAFICO.md`. Sin cambios de esquema ni migraciones.

### Yape y Plin con QR
- Yape y Plin como implementaciones independientes de `IMetodoPago`, registradas mediante inyección de dependencias y disponibles en el selector del checkout (OCP — sin condiciones adicionales en el coordinador).
- QR de cada billetera en el checkout, con archivos públicos `wwwroot/img/pagos/yape-qr.png` y `plin-qr.png`. Si falta una imagen, se muestra un aviso. Los pagos continúan siendo simulados.
- Pruebas de resolución de los cuatro métodos, rechazo, códigos inválidos, persistencia del método elegido, reversión transaccional y lectura de pedidos históricos. SQLite se usa únicamente como base temporal de pruebas; la tienda conserva MySQL.
- Cada pago nuevo guarda el método seleccionado: Tarjeta = 2, Transferencia = 3, Yape = 4 y Plin = 5. `Simulado = 1` conserva el significado de los registros anteriores.

### Flujo de compras
- Carrito para invitados y usuarios registrados; fusión al iniciar sesión; control de stock y operaciones transaccionales.
- Checkout con formulario de envío, selección de método de pago y confirmación con descuento de stock.
- Precarga del formulario con los datos del último envío del usuario; si no hay pedidos previos, usa el teléfono guardado en la cuenta.
- El administrador no puede acceder al carrito, al checkout ni a la vista de pedidos de la tienda.
- Teléfono de envío validado (exactamente 9 dígitos); botón de confirmación no queda bloqueado si el cliente corrige un error de validación.
- Precio del producto analizado sin depender de la cultura del sistema operativo anfitrión.

### Catálogo — mejoras
- Filtro de material en el catálogo: acepta `material` por GET y se combina con los demás filtros sin modificar `CatalogoService` (OCP — `FiltroMaterial` como clase independiente). El detalle del producto muestra nombre y descripción del material.
- Header de la tienda con enlaces por categoría (Anillos, Cadenas, Pulseras) y marcado activo con `aria-current="page"` al coincidir con la categoría actual.
- Filtros del catálogo aplicados automáticamente: los selects envían el formulario al cambiar (`change`); los campos de precio envían al salir (`change`) solo si el valor cambió. Sin botones intermedios.

### Registro y legales
- Corrección del formulario de registro: `AceptoTerminos` usa `MustBeTrue` para validar que el checkbox sea obligatorio.
- Términos y Condiciones y Política de Privacidad publicados (versión inicial, pendiente de contenido real).

### Backlog declarado
- ID 7 (UX del formulario de producto): parcialmente cubierto por la carga de imagen; pendiente slug automático desde el nombre, botón deshabilitado al enviar y mensajes de error visibles sin reselección del archivo.
- `.gitattributes`: pendiente para normalizar fin de línea entre sistemas operativos del equipo.
- Página de error personalizada (404 / 500): pendiente vista de error amigable.

---

## [0.1.0] - 2026-10-01

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
- Datos iniciales: 6 anillos, categorías, usuario admin y cliente de acceso.
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
