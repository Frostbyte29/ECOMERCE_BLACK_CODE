using System;
using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;

#nullable disable

namespace BlackCode.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AgregarFuncionesCuentaCliente : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "direcciones_guardadas",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    usuario_id = table.Column<int>(type: "int", nullable: false),
                    nombre_completo = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: false),
                    telefono = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                    direccion = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false),
                    distrito = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false),
                    referencia = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true),
                    es_predeterminada = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    creado_utc = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_direcciones_guardadas", x => x.id);
                    table.ForeignKey(
                        name: "FK_direcciones_guardadas_usuarios_usuario_id",
                        column: x => x.usuario_id,
                        principalTable: "usuarios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "reclamos",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    usuario_id = table.Column<int>(type: "int", nullable: true),
                    nombres_completos = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: false),
                    documento = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                    domicilio = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false),
                    correo = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false),
                    telefono = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true),
                    numero_pedido = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true),
                    tipo = table.Column<int>(type: "int", nullable: false),
                    descripcion_bien = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: false),
                    detalle = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: false),
                    pedido_cliente = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: false),
                    creado_utc = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_reclamos", x => x.id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "resenas",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    producto_id = table.Column<int>(type: "int", nullable: false),
                    usuario_id = table.Column<int>(type: "int", nullable: false),
                    calificacion = table.Column<int>(type: "int", nullable: false),
                    comentario = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true),
                    creado_utc = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_resenas", x => x.id);
                    table.ForeignKey(
                        name: "FK_resenas_productos_producto_id",
                        column: x => x.producto_id,
                        principalTable: "productos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_resenas_usuarios_usuario_id",
                        column: x => x.usuario_id,
                        principalTable: "usuarios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "seguimiento_pedido",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    pedido_id = table.Column<int>(type: "int", nullable: false),
                    transportista = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: true),
                    codigo_rastreo = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: true),
                    creado_utc = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_seguimiento_pedido", x => x.id);
                    table.ForeignKey(
                        name: "FK_seguimiento_pedido_pedidos_pedido_id",
                        column: x => x.pedido_id,
                        principalTable: "pedidos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_direcciones_guardadas_usuario_id",
                table: "direcciones_guardadas",
                column: "usuario_id");

            migrationBuilder.CreateIndex(
                name: "IX_reclamos_usuario_id",
                table: "reclamos",
                column: "usuario_id");

            migrationBuilder.CreateIndex(
                name: "IX_resenas_producto_id_usuario_id",
                table: "resenas",
                columns: new[] { "producto_id", "usuario_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_resenas_usuario_id",
                table: "resenas",
                column: "usuario_id");

            migrationBuilder.CreateIndex(
                name: "IX_seguimiento_pedido_pedido_id",
                table: "seguimiento_pedido",
                column: "pedido_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "direcciones_guardadas");

            migrationBuilder.DropTable(
                name: "reclamos");

            migrationBuilder.DropTable(
                name: "resenas");

            migrationBuilder.DropTable(
                name: "seguimiento_pedido");
        }
    }
}
