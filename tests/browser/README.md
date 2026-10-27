# Pruebas de navegador y concurrencia

Son opcionales para desarrollar con F5. Necesitan Node.js y Microsoft Edge instalado.
Usan únicamente la aplicación local https://localhost:7262 y la base de demostración
blackcode_prototipo en 127.0.0.1:3307.

Los scripts crean cuentas, carritos, pedidos y productos de prueba. No los ejecutes contra
una base real. Dejan los pedidos como evidencia y retiran del catálogo los productos temporales.

1. Inicia MySQL y el aplicativo.
2. Abre PowerShell en esta carpeta y ejecuta `npm ci`.
3. Configura en esa terminal las siguientes variables con tus valores locales. No las guardes en el repositorio:

| Variable | Contenido |
|---|---|
| BLACKCODE_MYSQL_BIN | Carpeta que contiene mysql.exe |
| BLACKCODE_MYSQL_USER | Usuario con permiso de lectura en blackcode_prototipo; por defecto blackcode_proto |
| BLACKCODE_MYSQL_PASSWORD | Contraseña de ese usuario |
| BLACKCODE_ADMIN_PASSWORD | Contraseña local de admin@blackcode.pe |
| BLACKCODE_CLIENTE_PASSWORD | Contraseña local de cliente@demo.pe |

4. Ejecuta `npm test` y `npm run test:concurrencia`.

Las capturas se guardan en docs/capturas. Los archivos locales de resultados se ignoran en Git.
Las pruebas de comparación de filtros suponen los seis anillos iniciales publicados.
Playwright acepta el certificado de desarrollo para las pruebas; el uso normal con F5
debe confiar en el certificado mediante dotnet dev-certs https --trust.
