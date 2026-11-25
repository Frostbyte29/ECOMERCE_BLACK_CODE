# Política comercial — claves y contrato de Application

Documento de referencia para quien implemente la pantalla de Configuración en el panel.

---

## Tabla de claves en `configuracion_tienda`

| Clave | Tipo | Valor por defecto | Descripción |
|---|---|---|---|
| `Shipping:FlatCost` | decimal | `15.00` | Costo fijo de envío en soles |
| `Shipping:FreeThreshold` | decimal | `199.00` | Subtotal mínimo para envío gratis |
| `Policy:ShippingDays` | texto | `2 a 4 días hábiles` | Plazo estimado de entrega que se muestra al cliente |
| `Policy:ReturnDays` | texto | `7 días calendario` | Plazo para cambios y devoluciones |
| `Banner:Message1` | texto | `ENVÍOS A TODO EL PERÚ` | Primer mensaje de la barra de anuncio (vacío = no mostrar) |
| `Banner:Message2` | texto | `CAMBIOS FÁCILES` | Segundo mensaje de la barra (vacío = no mostrar) |
| `Banner:Message3` | texto | `PAGA SEGURO` | Tercer mensaje de la barra (vacío = no mostrar) |
| `Contact:WhatsApp` | texto | `` | Número o enlace de WhatsApp (vacío = no mostrar) |
| `Social:Instagram` | texto | `` | URL completa de Instagram (vacío = no mostrar) |
| `Social:Tiktok` | texto | `` | URL completa de TikTok (vacío = no mostrar) |
| `Social:Facebook` | texto | `` | URL completa de Facebook (vacío = no mostrar) |

---

## Contrato de Application

### Namespace
`BlackCode.Application.Features.Configuracion`

### Interfaz
```csharp
public interface IPoliticaComercialService
{
    Task<PoliticaComercialDto> ObtenerAsync(CancellationToken ct = default);
}
```

### DTO de retorno
```csharp
public record PoliticaComercialDto(
    string PlazoDias,         // Policy:ShippingDays
    string PlazoCambios,      // Policy:ReturnDays
    decimal CostoEnvio,       // Shipping:FlatCost
    decimal UmbralGratis,     // Shipping:FreeThreshold
    IReadOnlyList<string> MensajesBarra,  // Banner:Message1-3 (vacíos excluidos)
    string? ContactoWhatsApp, // Contact:WhatsApp  — null si vacío
    string? InstagramUrl,     // Social:Instagram  — null si vacío
    string? TiktokUrl,        // Social:Tiktok     — null si vacío
    string? FacebookUrl);     // Social:Facebook   — null si vacío
```

### Registro en DI
```csharp
services.AddScoped<IPoliticaComercialService, PoliticaComercialService>();
```

---

## Consumidores actuales

| Pantalla / archivo | Qué lee |
|---|---|
| `Views/Shared/_Layout.cshtml` | `MensajesBarra` (barra de anuncio), `InstagramUrl`, `TiktokUrl`, `FacebookUrl` (pie) |
| `Views/Catalogo/Detalle.cshtml` | `PlazoDias`, `PlazoCambios` (service points del detalle) |
| `Views/Pedidos/Checkout.cshtml` | `PlazoDias` (junto al botón Confirmar) |
| `Views/Pedidos/Detalle.cshtml` | `PlazoDias` (sección Seguimiento) |
| `Views/Legal/Terminos.cshtml` | `PlazoDias`, `PlazoCambios` (secciones 5 y 6) |

---

## Vocabulario único de estados de pedido (T-14)

Definido en `TiendaMapper.Estado` (`BlackCode.Web/Models/TiendaViewModels.cs`).

| `EstadoPedido` | Texto visible |
|---|---|
| `PendientePago` | Pendiente de pago |
| `Pagado` | Pago confirmado |
| `EnPreparacion` | Preparando tu pedido |
| `Enviado` | En camino |
| `Entregado` | Entregado |
| `Cancelado` | Cancelado |
