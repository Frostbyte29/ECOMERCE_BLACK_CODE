# Abrir el prototipo con Visual Studio y MySQL

Este proyecto es una copia independiente de Frostbyte29/ECOMERCE_BLACK_CODE.
Conserva su historial y parte del commit de integración 8e868ce.
El desarrollo del prototipo está en la rama **angelo/prototipo**, propuesto hacia **develop** mediante Pull Request.

## En esta computadora

1. Abre **BlackCode.slnx** con **Visual Studio 2026**.
2. En el Explorador de soluciones, clic derecho en **BlackCode.Web → Establecer como proyecto de inicio**.
3. Selecciona el perfil **https** en la barra superior.
4. Comprueba que la instancia local de MySQL está encendida. Tras reiniciar Windows, ejecuta desde la carpeta del proyecto:

   ```powershell
   powershell -NoProfile -File tools/Start-LocalDatabase.ps1
   ```

5. Pulsa **F5**. La dirección del prototipo es **https://localhost:7262**.
6. Si aparece un aviso de certificado, ejecuta `dotnet dev-certs https --trust` y acepta el certificado de desarrollo de localhost.

La configuración preparada utiliza .NET SDK 10.0.402 y MySQL Community 8.4.11.
El servidor de demostración está aislado en el puerto 3307. No reemplaza el MySQL 8.0 instalado.
Los archivos de servidor y datos están fuera del repositorio; su ubicación se registra en el archivo local ignorado `.local/database.json`.
Conserva esas carpetas si quieres seguir utilizando esta instancia. El servidor no está registrado como servicio de inicio automático.

La cadena de conexión y las contraseñas se guardan únicamente en los **secretos de usuario de BlackCode.Web**.
Este prototipo tiene su propio identificador de secretos: `blackcode-prototipo-angelo-2026`.
No comparte las cookies ni los puertos HTTP/HTTPS del repositorio original.

## Cuentas de demostración

| Cuenta | Rol | Dónde consultar la contraseña local |
|---|---|---|
| admin@blackcode.pe | Administrador | Secretos de usuario → Seed → AdminPassword |
| cliente@demo.pe | Cliente | Secretos de usuario → Seed → ClientePassword |

En Visual Studio: clic derecho en **BlackCode.Web → Administrar secretos de usuario**.
Las contraseñas se generaron para esta instalación y no están publicadas en GitHub.
También puedes registrar una cuenta de cliente desde la propia pantalla del aplicativo.
Cambiar Seed después de la primera carga no cambia contraseñas de usuarios que ya existen.

## Ver la base desde MySQL Workbench

Crea una conexión nueva de tipo **Standard (TCP/IP)**:

| Campo | Valor local |
|---|---|
| Connection Name | BlackCODE Prototipo |
| Hostname | 127.0.0.1 |
| Port | 3307 |
| Username | blackcode_proto |
| Password | Valor de Pwd en ConnectionStrings:Default, dentro de los secretos de usuario |
| Default Schema | blackcode_prototipo |

Pulsa **Test Connection**, conecta y actualiza **Schemas**.
Puedes consultar el avance con:

```sql
SELECT VERSION();
SELECT id, nombre, precio, estado FROM productos;
SELECT id, producto_id, talla, stock FROM variantes_producto;
SELECT id, usuario_id, token_anonimo FROM carritos;
SELECT id, codigo, usuario_id, estado, total FROM pedidos;
SELECT pedido_id, estado_anterior, estado_nuevo, cambiado_utc
FROM historial_estado_pedido ORDER BY id DESC;
```

La instancia conserva los pedidos de demostración generados al verificar el aplicativo.
Todos los pagos son simulados; no introduzcas información real de tarjetas.

## En otra computadora

1. Instala Visual Studio 2026 con la carga **Desarrollo de ASP.NET y web**, SDK .NET 10 y MySQL 8.4.
2. Clona **AngeloE2003/BlackCODE-Prototipo** y selecciona `angelo/prototipo` mientras el PR no esté integrado.
3. Crea una base y usuario exclusivos del prototipo. Usa `blackcode_prototipo` como nombre de base para distinguirlo del proyecto del equipo.
4. En secretos de usuario, configura `ConnectionStrings:Default`, `Seed:AdminPassword` y `Seed:ClientePassword`, como se describe en el README.
5. Ajusta host, puerto, base y usuario a tu instalación. El puerto 3307 corresponde únicamente a la instalación local preparada aquí.
6. Ejecuta con el perfil https. En Development, el arranque aplica la migración existente y carga ocho productos iniciales.

## Recorrido para la exposición

1. Inicio: estilo blanco, negro y gris; menú desplegable y barra fija.
2. Busca “anillo”: los filtros permanecen a la izquierda.
3. Abre una pieza, selecciona talla y agrégala sin iniciar sesión.
4. Cambia la cantidad y prueba un valor mayor al stock.
5. Continúa la compra, regístrate o inicia sesión; el carrito se conserva.
6. Completa el envío y prueba **Simular un pago rechazado**: el stock y el carrito se mantienen.
7. Desmarca esa opción y confirma. Se crea el pedido, se descuenta stock y se vacía el carrito.
8. Desde el mismo login, entra como administrador. Modifica un producto y actualiza el estado del pedido.
9. En Workbench, consulta las tablas para mostrar los registros persistidos.

## Alcance de la validación

Se compiló con .NET 10 y se ejecutó con el mismo perfil https definido para Visual Studio.
Las pruebas automatizadas y de navegador se documentan en **VALIDACION.md**.
No se automatizó la pulsación de F5 ni la interfaz de Visual Studio; la selección del proyecto de inicio es el paso manual indicado arriba.
