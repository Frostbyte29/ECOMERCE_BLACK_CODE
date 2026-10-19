# Changelog

Formato: [Keep a Changelog](https://keepachangelog.com/es-ES/1.1.0/) · Versionado: [SemVer](https://semver.org/lang/es/)

## [Sin publicar]

### Agregado
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
