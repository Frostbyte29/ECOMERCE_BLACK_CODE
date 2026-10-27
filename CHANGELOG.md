# Changelog

Formato: [Keep a Changelog](https://keepachangelog.com/es-ES/1.1.0/) · Versionado: [SemVer](https://semver.org/lang/es/)

## [Sin publicar]

### Agregado
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

### Eliminado
- Enlace de recuperación de contraseña del formulario de login (se implementará por correo más adelante).

## [0.1.0]
