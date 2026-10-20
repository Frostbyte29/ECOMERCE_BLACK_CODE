# Módulo de pedidos y pagos

Carrito, flujo de compra, pagos simulados, historial de pedidos del cliente y panel de gestión para administradores.

## Rutas

| Ruta | Quién | Qué hace |
|---|---|---|
| `/Carrito` | Todos | Ver, actualizar y quitar productos (el carrito vive en la sesión) |
| `/Checkout` | Logueado | Paso 1: datos de envío → crea el pedido en estado *Pendiente de pago* |
| `/Checkout/Pagar/{id}` | Dueño del pedido | Paso 2: pago (tarjeta, Yape/Plin o contra entrega); se puede reintentar |
| `/Checkout/Confirmacion/{id}` | Dueño del pedido | Paso 3: resumen del pedido |
| `/Pedidos` | Logueado | Historial de pedidos del usuario |
| `/Pedidos/Detalle/{id}` | Dueño del pedido | Detalle, pagos, seguimiento; cancelar si aún no se pagó |
| `/Admin/Pedidos` | Rol `Admin` | Panel: ventas del día/mes, filtros por estado, búsqueda, fechas y paginación |
| `/Admin/Pedidos/{id}` | Rol `Admin` | Cambiar estado, n.º de seguimiento y notas |

## Estados del pedido

```
PendientePago ──pago aprobado──▶ Pagado ──▶ EnPreparacion ──▶ Enviado ──▶ Entregado
      │         (contra entrega)──────────▶ EnPreparacion
      └──▶ Cancelado ◀── (desde Pagado o EnPreparacion; los pagos aprobados pasan a Reembolsado)
```

- Las transiciones permitidas están en `OrderService.Transitions`.
- Cada cambio queda registrado en `OrderStatusHistory` (quién, cuándo y nota).
- En contra entrega el pago queda *Pendiente* y se aprueba al marcar el pedido como *Entregado*.

## Pagos simulados

`OrderService.ProcessPaymentAsync` hace de pasarela falsa. No se guarda el número de tarjeta, solo los últimos 4 dígitos.

- Tarjeta `4111 1111 1111 1111`, cualquier vencimiento futuro y CVV de 3 dígitos → **aprobado**
- Tarjeta `4000 0000 0000 0002` → **rechazado** (fondos insuficientes)
- Yape/Plin: cualquier número de operación de 6 a 10 dígitos → **aprobado**

Para integrar una pasarela real (Culqi, Niubiz, Mercado Pago) solo hay que reemplazar ese método.

## Archivos

- Modelos: `Models/Order.cs`, `Models/Payment.cs`, `Models/ApplicationUser.cs`, `Models/CartViewModels.cs`
- Lógica: `Services/CartService.cs`, `Services/OrderService.cs`, `Services/PeruTime.cs`
- Controladores: `CarritoController`, `CheckoutController`, `PedidosController`, `AdminPedidosController`
- Vistas: `Views/Carrito`, `Views/Checkout`, `Views/Pedidos`, `Views/AdminPedidos`, `Views/Shared/_Order*.cshtml`
- Estilos: `wwwroot/css/pedidos.css` (separado de `site.css`)
- Migración: `Data/Migrations/*_InicialPedidosPagos.cs`. `pedidos_pagos.sql` está incompleto y no crea el esquema completo; utiliza `dotnet ef database update`.

## Usuarios (temporal)

Para poder probar, se agregó ASP.NET Core Identity con roles `Admin` y `Cliente`, y un login mínimo en
`CuentaController` (`/Cuenta/Login`, `/Cuenta/Registro`, `/Cuenta/Logout`). El módulo de usuarios del equipo
puede reemplazar ese controlador y sus vistas manteniendo esas rutas.
Al arrancar se puede crear un administrador de prueba si `SeedAdmin:Email` y `SeedAdmin:Password` están configurados en User Secrets o variables de entorno. No se incluyen credenciales en `appsettings.json`.

## Cómo levantarlo

1. Guarda tu conexión a MySQL sin tocar `appsettings.json`:
   `dotnet user-secrets set "ConnectionStrings:BlackCode" "Server=localhost;Port=3306;Database=blackcode_db;User=root;Password=TU_CLAVE;"`
2. Crea las tablas: `dotnet ef database update` (dentro de `BlackCode.Web`)
3. `dotnet run`

> Si tu base ya tiene la tabla `Products` creada a mano, borra la base (`DROP DATABASE blackcode_db;`) antes del paso 2,
> o ejecuta del script `pedidos_pagos.sql` solo la parte que no sea `Products`.

## Pendiente / ideas

- No hay control de stock (el producto no tiene campo de stock todavía).
- Enviar correo de confirmación al crear o cambiar el estado del pedido.
