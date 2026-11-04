using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace RaizesNordeste.Infra.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Clientes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Nome = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Email = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Cpf = table.Column<string>(type: "character varying(14)", maxLength: 14, nullable: false),
                    Pontos = table.Column<int>(type: "integer", nullable: false),
                    ConsenteTermosLgpd = table.Column<bool>(type: "boolean", nullable: false),
                    Anonimo = table.Column<bool>(type: "boolean", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clientes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Unidades",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Nome = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Cidade = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    CozinhaCompleta = table.Column<bool>(type: "boolean", nullable: false),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Unidades", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Itens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    IdUnidade = table.Column<Guid>(type: "uuid", nullable: false),
                    Nome = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Descricao = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Preco = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    Disponivel = table.Column<bool>(type: "boolean", nullable: false),
                    ProdutoSazonal = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Itens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Itens_Unidades_IdUnidade",
                        column: x => x.IdUnidade,
                        principalTable: "Unidades",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Ocorrencias",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    IdUnidade = table.Column<Guid>(type: "uuid", nullable: true),
                    TipoAcao = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    IdAfetado = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Ator = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Detalhes = table.Column<string>(type: "text", nullable: false),
                    DataOcorrencia = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ocorrencias", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Ocorrencias_Unidades_IdUnidade",
                        column: x => x.IdUnidade,
                        principalTable: "Unidades",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Pedidos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    IdCliente = table.Column<Guid>(type: "uuid", nullable: true),
                    IdUnidade = table.Column<Guid>(type: "uuid", nullable: false),
                    Canal = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    ValorTotal = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    IdTransacaoExterna = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    MotivoCancelamento = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    DataCriacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pedidos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Pedidos_Clientes_IdCliente",
                        column: x => x.IdCliente,
                        principalTable: "Clientes",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Pedidos_Unidades_IdUnidade",
                        column: x => x.IdUnidade,
                        principalTable: "Unidades",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ItensPedido",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    IdPedido = table.Column<Guid>(type: "uuid", nullable: false),
                    IdItem = table.Column<Guid>(type: "uuid", nullable: false),
                    Quantidade = table.Column<int>(type: "integer", nullable: false),
                    PrecoUnitario = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItensPedido", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ItensPedido_Itens_IdItem",
                        column: x => x.IdItem,
                        principalTable: "Itens",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ItensPedido_Pedidos_IdPedido",
                        column: x => x.IdPedido,
                        principalTable: "Pedidos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Unidades",
                columns: new[] { "Id", "Ativo", "Cidade", "CozinhaCompleta", "Nome" },
                values: new object[] { new Guid("99999999-8888-7777-6666-555555555555"), true, "Fortaleza", true, "Raízes do Nordeste - Filial Centro" });

            migrationBuilder.InsertData(
                table: "Itens",
                columns: new[] { "Id", "Descricao", "Disponivel", "IdUnidade", "Nome", "Preco", "ProdutoSazonal" },
                values: new object[,]
                {
                    { new Guid("11111111-2222-3333-4444-555555555555"), "Arroz, feijão de corda, queijo coalho assado e carne de sol na nata.", true, new Guid("99999999-8888-7777-6666-555555555555"), "Baião de Dois Tradicional", 34.90m, false },
                    { new Guid("22222222-3333-4444-5555-666666666666"), "Cuscuz de milho flocão recheado com carne seca desfiada e requeijão do norte.", true, new Guid("99999999-8888-7777-6666-555555555555"), "Cuscuz Recheado Sertanejo", 22.50m, false },
                    { new Guid("33333333-4444-5555-6666-777777777777"), "Canjica de milho verde com coco e canela (Cardápio Sazonal São João).", true, new Guid("99999999-8888-7777-6666-555555555555"), "Canjica Junina Especial", 14.00m, true }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Clientes_Cpf",
                table: "Clientes",
                column: "Cpf",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Itens_IdUnidade",
                table: "Itens",
                column: "IdUnidade");

            migrationBuilder.CreateIndex(
                name: "IX_ItensPedido_IdItem",
                table: "ItensPedido",
                column: "IdItem");

            migrationBuilder.CreateIndex(
                name: "IX_ItensPedido_IdPedido",
                table: "ItensPedido",
                column: "IdPedido");

            migrationBuilder.CreateIndex(
                name: "IX_Ocorrencias_IdUnidade",
                table: "Ocorrencias",
                column: "IdUnidade");

            migrationBuilder.CreateIndex(
                name: "IX_Pedidos_IdCliente",
                table: "Pedidos",
                column: "IdCliente");

            migrationBuilder.CreateIndex(
                name: "IX_Pedidos_IdUnidade",
                table: "Pedidos",
                column: "IdUnidade");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ItensPedido");

            migrationBuilder.DropTable(
                name: "Ocorrencias");

            migrationBuilder.DropTable(
                name: "Itens");

            migrationBuilder.DropTable(
                name: "Pedidos");

            migrationBuilder.DropTable(
                name: "Clientes");

            migrationBuilder.DropTable(
                name: "Unidades");
        }
    }
}
