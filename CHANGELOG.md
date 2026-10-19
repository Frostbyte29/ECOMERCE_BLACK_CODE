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

## [0.1.0]
