# Catálogo con fotografías

Se incorporan 19 diseños de las fotografías facilitadas. Los nombres, precios y materiales son propuestos para la demostración; las fotografías no certifican la composición. Cada anillo comienza con tallas 7, 8, 9, 10 y 11, y 10 unidades por talla. Revisar estos datos antes de usarlos para ventas reales.

| Producto | Precio sugerido (S/) | Fotografía original |
|---|---:|---|
| Anillo Carmesí | 69.00 | PXL_20221015_165631430.jpg |
| Anillo Obsidiana | 65.00 | PXL_20221015_172200352.jpg |
| Anillo Hexagrama | 59.00 | PXL_20221015_172643090.jpg |
| Anillo Celta | 59.00 | PXL_20221015_173141550.jpg |
| Anillo Brújula | 65.00 | PXL_20221015_173645425.jpg |
| Anillo Osario | 55.00 | PXL_20221015_174025223.jpg |
| Anillo Escudo | 55.00 | PXL_20221015_174326600.jpg |
| Anillo Aries | 75.00 | PXL_20221015_174607849.jpg |
| Anillo Dragón | 79.00 | PXL_20221015_174631089.jpg |
| Anillo Prisma | 59.00 | PXL_20221015_175257677.jpg |
| Anillo Imperio | 75.00 | PXL_20221015_175614845.jpg |
| Anillo Medianoche | 49.00 | PXL_20221015_175921013.jpg |
| Anillo Roseta | 65.00 | PXL_20221015_180104091.jpg |
| Anillo Templario | 55.00 | PXL_20221015_180257518.jpg |
| Anillo Memento | 69.00 | PXL_20221015_180541717.jpg |
| Anillo Azabache | 69.00 | PXL_20221015_180703762.jpg |
| Anillo Cobra | 75.00 | PXL_20221015_180846718.jpg |
| Anillo Náutico | 59.00 | PXL_20221015_181017976.jpg |
| Anillo Equilibrio | 59.00 | PXL_20221015_181332120.jpg |

## Aplicación local

Al iniciar en Development, el catálogo se incorpora una sola vez, identificado por la clave Catalogo:Fotografias:v1 en configuracion_tienda. No se crean tablas ni migraciones. Los productos iniciales que aún conservan la ilustración original se retiran a borrador, sin borrar variantes ni pedidos. Los cambios posteriores del administrador en precios, tallas, stock, fotos o estado no se sobrescriben al reiniciar.

## Imágenes de productos

Administración → Crear o editar producto permite elegir JPG, PNG o WebP de hasta 5 MB y previsualizarlo antes de guardar. Crear exige una fotografía; editar conserva la actual si no se selecciona otra. Un error de formulario exige volver a seleccionar el archivo. El servidor valida extensión, firma y límite de tamaño; los nombres físicos son aleatorios.

Las fotografías iniciales están en BlackCode.Web/wwwroot/img/productos/catalogo. Las cargas del administrador se guardan en BlackCode.Web/wwwroot/img/productos/cargas. Estas últimas deben respaldarse junto con la base de datos y conservarse al desplegar nuevas versiones. No son almacenadas dentro de MySQL: la base conserva su URL. Si falla la validación del producto se elimina únicamente la carga nueva. Las imágenes anteriores no se borran automáticamente, para conservar otras referencias.

## Comprobación manual

1. Iniciar en Development y revisar los 19 anillos en el catálogo y los destacados en Inicio.
2. Filtrar por color, material, talla y precio; abrir el detalle y comprobar las variantes.
3. Crear un producto con una imagen y observar la vista previa antes de guardar.
4. Editarlo sin elegir foto; después sustituir la foto y comprobar el catálogo.
5. Intentar cargar un archivo no permitido o de más de 5 MB; debe rechazarse.
6. Cambiar precio y stock; reiniciar y comprobar que se conservan.
7. Revisar nombre y precio juntos en escritorio y móvil.
