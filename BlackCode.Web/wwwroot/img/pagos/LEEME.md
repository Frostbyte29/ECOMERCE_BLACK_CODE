# Imágenes QR de pago

Colocar los archivos PNG originales, con estos nombres exactos:

- `yape-qr.png`
- `plin-qr.png`

La aplicación los sirve en `/img/pagos/yape-qr.png` y `/img/pagos/plin-qr.png`.
Al elegir Yape o Plin en el checkout aparece únicamente el QR correspondiente.
Si falta un archivo, se muestra un aviso en lugar de una imagen rota.

Conservar el QR completo, su margen blanco y su proporción. No redibujarlo ni
recortarlo. Se recomienda una imagen de al menos 400 píxeles de ancho.

Los archivos dentro de `wwwroot` son públicos y se incluyen al publicar el proyecto
Web. Para que estén también en GitHub, deben formar parte del futuro commit y PR.
Al reemplazarlos, el checkout actualiza la URL de caché mediante `asp-append-version`.

Los métodos de esta RFC son simulados. Mostrar un QR no valida una transferencia
ni conecta con las aplicaciones de Yape o Plin. No realizar transferencias en la demo.
