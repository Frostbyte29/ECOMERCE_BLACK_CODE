using System;
using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;

#nullable disable

namespace BlackCode.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "categorias",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    nombre = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false),
                    slug = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false),
                    descripcion = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true),
                    url_imagen = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: true),
                    orden = table.Column<int>(type: "int", nullable: false),
                    activo = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_categorias", x => x.id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "colores",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    nombre = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false),
                    codigo_hex = table.Column<string>(type: "varchar(7)", maxLength: 7, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_colores", x => x.id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "configuracion_tienda",
                columns: table => new
                {
                    clave = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false),
                    valor = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_configuracion_tienda", x => x.clave);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "materiales",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    nombre = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false),
                    descripcion = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_materiales", x => x.id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "usuarios",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    correo = table.Column<string>(type: "varchar(254)", maxLength: 254, nullable: false),
                    hash_contrasena = table.Column<string>(type: "varchar(512)", maxLength: 512, nullable: false),
                    nombre_completo = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: false),
                    telefono = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true),
                    rol = table.Column<int>(type: "int", nullable: false),
                    activo = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    intentos_fallidos = table.Column<int>(type: "int", nullable: false),
                    bloqueado_hasta_utc = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    terminos_aceptados_utc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    creado_utc = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_usuarios", x => x.id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "productos",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    nombre = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: false),
                    slug = table.Column<string>(type: "varchar(140)", maxLength: 140, nullable: false),
                    descripcion = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: false),
                    precio = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    acabado = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: true),
                    destacado = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    estado = table.Column<int>(type: "int", nullable: false),
                    creado_utc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    categoria_id = table.Column<int>(type: "int", nullable: false),
                    material_id = table.Column<int>(type: "int", nullable: false),
                    color_id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_productos", x => x.id);
                    table.ForeignKey(
                        name: "FK_productos_categorias_categoria_id",
                        column: x => x.categoria_id,
                        principalTable: "categorias",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_productos_colores_color_id",
                        column: x => x.color_id,
                        principalTable: "colores",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_productos_materiales_material_id",
                        column: x => x.material_id,
                        principalTable: "materiales",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "carritos",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    usuario_id = table.Column<int>(type: "int", nullable: true),
                    token_anonimo = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: true),
                    actualizado_utc = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_carritos", x => x.id);
                    table.ForeignKey(
                        name: "FK_carritos_usuarios_usuario_id",
                        column: x => x.usuario_id,
                        principalTable: "usuarios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "pedidos",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    codigo = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                    usuario_id = table.Column<int>(type: "int", nullable: false),
                    estado = table.Column<int>(type: "int", nullable: false),
                    subtotal = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    costo_envio = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    total = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    envio_nombre_completo = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: false),
                    envio_telefono = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                    envio_direccion = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false),
                    envio_distrito = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false),
                    envio_referencia = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true),
                    creado_utc = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pedidos", x => x.id);
                    table.ForeignKey(
                        name: "FK_pedidos_usuarios_usuario_id",
                        column: x => x.usuario_id,
                        principalTable: "usuarios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "favoritos",
                columns: table => new
                {
                    usuario_id = table.Column<int>(type: "int", nullable: false),
                    producto_id = table.Column<int>(type: "int", nullable: false),
                    creado_utc = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_favoritos", x => new { x.usuario_id, x.producto_id });
                    table.ForeignKey(
                        name: "FK_favoritos_productos_producto_id",
                        column: x => x.producto_id,
                        principalTable: "productos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_favoritos_usuarios_usuario_id",
                        column: x => x.usuario_id,
                        principalTable: "usuarios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "imagenes_producto",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    producto_id = table.Column<int>(type: "int", nullable: false),
                    url = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: false),
                    orden = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_imagenes_producto", x => x.id);
                    table.ForeignKey(
                        name: "FK_imagenes_producto_productos_producto_id",
                        column: x => x.producto_id,
                        principalTable: "productos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "variantes_producto",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    producto_id = table.Column<int>(type: "int", nullable: false),
                    talla = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                    sku = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false),
                    stock = table.Column<int>(type: "int", nullable: false),
                    activo = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_variantes_producto", x => x.id);
                    table.ForeignKey(
                        name: "FK_variantes_producto_productos_producto_id",
                        column: x => x.producto_id,
                        principalTable: "productos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "historial_estado_pedido",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    pedido_id = table.Column<int>(type: "int", nullable: false),
                    estado_anterior = table.Column<int>(type: "int", nullable: false),
                    estado_nuevo = table.Column<int>(type: "int", nullable: false),
                    cambiado_por_usuario_id = table.Column<int>(type: "int", nullable: false),
                    cambiado_utc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    nota = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_historial_estado_pedido", x => x.id);
                    table.ForeignKey(
                        name: "FK_historial_estado_pedido_pedidos_pedido_id",
                        column: x => x.pedido_id,
                        principalTable: "pedidos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "pagos",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    pedido_id = table.Column<int>(type: "int", nullable: false),
                    metodo = table.Column<int>(type: "int", nullable: false),
                    estado = table.Column<int>(type: "int", nullable: false),
                    monto = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    referencia = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false),
                    creado_utc = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pagos", x => x.id);
                    table.ForeignKey(
                        name: "FK_pagos_pedidos_pedido_id",
                        column: x => x.pedido_id,
                        principalTable: "pedidos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "items_carrito",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    carrito_id = table.Column<int>(type: "int", nullable: false),
                    variante_id = table.Column<int>(type: "int", nullable: false),
                    cantidad = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_items_carrito", x => x.id);
                    table.ForeignKey(
                        name: "FK_items_carrito_carritos_carrito_id",
                        column: x => x.carrito_id,
                        principalTable: "carritos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_items_carrito_variantes_producto_variante_id",
                        column: x => x.variante_id,
                        principalTable: "variantes_producto",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "items_pedido",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    pedido_id = table.Column<int>(type: "int", nullable: false),
                    variante_id = table.Column<int>(type: "int", nullable: false),
                    nombre_producto_copia = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: false),
                    talla_copia = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                    precio_unitario = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    cantidad = table.Column<int>(type: "int", nullable: false),
                    total_linea = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_items_pedido", x => x.id);
                    table.ForeignKey(
                        name: "FK_items_pedido_pedidos_pedido_id",
                        column: x => x.pedido_id,
                        principalTable: "pedidos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_items_pedido_variantes_producto_variante_id",
                        column: x => x.variante_id,
                        principalTable: "variantes_producto",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_carritos_token_anonimo",
                table: "carritos",
                column: "token_anonimo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_carritos_usuario_id",
                table: "carritos",
                column: "usuario_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_categorias_slug",
                table: "categorias",
                column: "slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_colores_nombre",
                table: "colores",
                column: "nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_favoritos_producto_id",
                table: "favoritos",
                column: "producto_id");

            migrationBuilder.CreateIndex(
                name: "IX_historial_estado_pedido_pedido_id",
                table: "historial_estado_pedido",
                column: "pedido_id");

            migrationBuilder.CreateIndex(
                name: "IX_imagenes_producto_producto_id",
                table: "imagenes_producto",
                column: "producto_id");

            migrationBuilder.CreateIndex(
                name: "IX_items_carrito_carrito_id_variante_id",
                table: "items_carrito",
                columns: new[] { "carrito_id", "variante_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_items_carrito_variante_id",
                table: "items_carrito",
                column: "variante_id");

            migrationBuilder.CreateIndex(
                name: "IX_items_pedido_pedido_id",
                table: "items_pedido",
                column: "pedido_id");

            migrationBuilder.CreateIndex(
                name: "IX_items_pedido_variante_id",
                table: "items_pedido",
                column: "variante_id");

            migrationBuilder.CreateIndex(
                name: "IX_materiales_nombre",
                table: "materiales",
                column: "nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_pagos_pedido_id",
                table: "pagos",
                column: "pedido_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_pedidos_codigo",
                table: "pedidos",
                column: "codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_pedidos_usuario_id",
                table: "pedidos",
                column: "usuario_id");

            migrationBuilder.CreateIndex(
                name: "IX_productos_categoria_id",
                table: "productos",
                column: "categoria_id");

            migrationBuilder.CreateIndex(
                name: "IX_productos_color_id",
                table: "productos",
                column: "color_id");

            migrationBuilder.CreateIndex(
                name: "IX_productos_estado",
                table: "productos",
                column: "estado");

            migrationBuilder.CreateIndex(
                name: "IX_productos_material_id",
                table: "productos",
                column: "material_id");

            migrationBuilder.CreateIndex(
                name: "IX_productos_slug",
                table: "productos",
                column: "slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_usuarios_correo",
                table: "usuarios",
                column: "correo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_variantes_producto_producto_id_talla",
                table: "variantes_producto",
                columns: new[] { "producto_id", "talla" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_variantes_producto_sku",
                table: "variantes_producto",
                column: "sku",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "configuracion_tienda");

            migrationBuilder.DropTable(
                name: "favoritos");

            migrationBuilder.DropTable(
                name: "historial_estado_pedido");

            migrationBuilder.DropTable(
                name: "imagenes_producto");

            migrationBuilder.DropTable(
                name: "items_carrito");

            migrationBuilder.DropTable(
                name: "items_pedido");

            migrationBuilder.DropTable(
                name: "pagos");

            migrationBuilder.DropTable(
                name: "carritos");

            migrationBuilder.DropTable(
                name: "variantes_producto");

            migrationBuilder.DropTable(
                name: "pedidos");

            migrationBuilder.DropTable(
                name: "productos");

            migrationBuilder.DropTable(
                name: "usuarios");

            migrationBuilder.DropTable(
                name: "categorias");

            migrationBuilder.DropTable(
                name: "colores");

            migrationBuilder.DropTable(
                name: "materiales");
        }
    }
}
