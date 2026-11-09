# Verificación del prototipo

Fecha de revisión: 19 de octubre de 2026.

## Entorno

- SDK .NET 10.0.402.
- MySQL Community 8.4.11, instancia independiente en 127.0.0.1:3307.
- Ejecución con el perfil https de BlackCode.Web, puerto 7262.
- Navegador Microsoft Edge mediante Playwright; escritorio 1440 × 1000 y móvil 390 × 844.

## Resultados

- Compilación correcta: cero errores y cero advertencias.
- **28 pruebas xUnit aprobadas**: extensibilidad SOLID en los cinco módulos, autenticación, reportes, transiciones de pedidos, umbral de envío, carrito vacío, reglas de descuento, aislamiento de invitados, control de cantidad y fusión de carritos.
- **20 comprobaciones del recorrido en navegador aprobadas**, que incluyen:
  - Inicio, menú al pasar el cursor y navbar fijo.
  - Búsqueda, filtros laterales y ausencia de resultados.
  - Carrito de invitado, validación de stock y conservación al registrarse.
  - Pago rechazado: sin pedido, sin descuento de stock, carrito conservado.
  - Pago aprobado con transferencia simulada: pedido persistido, stock descontado y carrito vacío.
  - Cliente sin acceso administrativo y sin acceso a pedidos de otro cliente.
  - Rechazo de formularios sin token antifalsificación.
  - Login administrativo, reportes CSV/JSON, actualización de estados y cancelación con reposición.
  - Creación, edición y eliminación de un producto de prueba.
  - Pantallas móviles sin desbordamiento horizontal y filtros a la izquierda.
  - Sin errores JavaScript ni respuestas HTTP 500 en el recorrido.
- **4 verificaciones de concurrencia sobre MySQL aprobadas**:
  - Dos compradores y una unidad: se confirma una sola compra.
  - Dos cancelaciones simultáneas: se repone una sola unidad.
  - Dos envíos simultáneos del formulario: se registra un solo pedido.
  - Un producto con pedidos se retira como borrador y conserva el historial.

## Cómo repetir

```powershell
dotnet build BlackCode.slnx
dotnet test BlackCode.Tests/BlackCode.Tests.csproj
```

Los scripts de navegador están en `tests/browser`. Requieren Node.js, el paquete Playwright y Edge instalado.
Consulta su README; usan variables de entorno para recibir credenciales y deben ejecutarse únicamente contra una base de demostración.

## Límites

- Se verificó el arranque por `dotnet run --project BlackCode.Web --launch-profile https`; no se automatizó el IDE ni la pulsación física de F5.
- Las imágenes son ilustraciones SVG locales de demostración. Pueden sustituirse por fotografías propias.
- El pago es simulado; no hay pasarela real, envío de correos ni logística real.
- El carrito admite invitados; para confirmar el pedido se exige una cuenta, conforme al vínculo obligatorio de Pedido con Usuario del modelo actual.
- No hay promociones activas. La regla DescuentoPorCantidad se incluye como ejemplo OCP y se verifica mediante pruebas.
- El CRUD permite crear productos con una variante y editar las variantes ya existentes. Un editor para añadir o quitar tallas a productos existentes queda fuera de este prototipo.
- No se modificaron ni se añadieron migraciones. Se conservó InitialSchema.
- No se ha publicado la aplicación en un servidor público; GitHub contiene el código.
# Actualización de identidad visual

Se incorporaron dos fotografías editoriales y las versiones blanca y negra del emblema
proporcionadas para BlackCODE. Se conservaron los archivos originales y su transparencia.
La marca se presenta desde `_Marca.cshtml`; `marca.css` selecciona el contraste según el fondo.

Verificación de esta actualización:
- `dotnet build BlackCode.slnx --no-restore`: 0 errores y 0 advertencias.
- `dotnet test BlackCode.slnx --no-build --no-restore`: 28 pruebas aprobadas.
- Navegador: inicio, login, registro, catálogo y carrito responden correctamente; imágenes locales
  cargadas y contraste del logo comprobado. Inicio, login y catálogo sin desbordamiento a 390 px.
- Capturas: `capturas/marca-inicio.png`, `capturas/marca-login.png` y `capturas/marca-movil.png`.

Esta actualización incorpora los recursos de marca; no representa una reproducción verificada
del archivo de Figma. La comparación detallada de sus pantallas sigue pendiente.

