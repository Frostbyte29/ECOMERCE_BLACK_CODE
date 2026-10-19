USE blackcode_db;

-- Actualiza los cuatro productos de demostración existentes.
UPDATE Products SET Name='Anillo Obsidiana', Price=49.90, Category='Anillos', Description='Diseño sobrio de líneas definidas. Una pieza versátil para el día a día.', Material='Acero inoxidable 316L', Finish='Negro mate', SizesCsv='7,8,9,10,11', ImageUrl='/images/products/anillo-obsidiana.jpg' WHERE Id=1;
UPDATE Products SET Name='Cadena Atlas', Price=69.90, Category='Cadenas', Description='Cadena de presencia discreta y acabado pulido, pensada para combinar con cualquier estilo.', Material='Acero inoxidable 316L', Finish='Plateado pulido', SizesCsv='Única', ImageUrl='/images/products/cadena-atlas.jpg' WHERE Id=2;
UPDATE Products SET Name='Pulsera Nox', Price=59.90, Category='Pulseras', Description='Una silueta minimalista con carácter urbano y cierre práctico.', Material='Acero inoxidable 316L', Finish='Negro y plateado', SizesCsv='Única', ImageUrl='/images/products/pulsera-nox.jpg' WHERE Id=3;
UPDATE Products SET Name='Collar Eclipse', Price=74.90, Category='Collares', Description='Un colgante de estética limpia que aporta un detalle sutil al conjunto.', Material='Acero inoxidable 316L', Finish='Negro satinado', SizesCsv='Única', ImageUrl='/images/products/collar-eclipse.jpg' WHERE Id=4;

-- Agrega el quinto producto si todavía no existe.
INSERT INTO Products (Id, Name, Price, Category, Description, Material, Finish, SizesCsv, ImageUrl)
SELECT 5, 'Anillo Monolito', 54.90, 'Anillos', 'Perfil geométrico y acabado metálico para un estilo atemporal.', 'Acero inoxidable 316L', 'Plateado cepillado', '7,8,9,10,11', '/images/products/anillo-monolito.jpg'
WHERE NOT EXISTS (SELECT 1 FROM Products WHERE Id=5);

SELECT Id, Name, Price, Category, ImageUrl FROM Products ORDER BY Id;
