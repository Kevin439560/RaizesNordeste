using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace RaizesNordeste.Infra.Migrations
{
    /// <inheritdoc />
    public partial class AjustesRoteiroTutoria : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "Desconto",
                table: "Pedidos",
                type: "numeric(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "FormaPagamento",
                table: "Pedidos",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "QuantidadeEstoque",
                table: "Itens",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "perfilUsuario",
                table: "Clientes",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "senhaHash",
                table: "Clientes",
                type: "character varying(128)",
                maxLength: 128,
                nullable: false,
                defaultValue: "");

            migrationBuilder.InsertData(
                table: "Clientes",
                columns: new[] { "Id", "Anonimo", "ConsenteTermosLgpd", "Cpf", "DataCriacao", "Email", "Nome", "Pontos", "perfilUsuario", "senhaHash" },
                values: new object[,]
                {
                    { new Guid("aaaaaaaa-bbbb-cccc-dddd-111111111111"), false, true, "111.111.111-11", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "cliente@raizes.com", "Cliente Teste Seed", 100, "CLIENTE", "8D969EEF6ECAD3C29A3A629280E686CF0C3F5D5A86AFF3CA12020C923ADC6C92" },
                    { new Guid("aaaaaaaa-bbbb-cccc-dddd-222222222222"), false, true, "999.999.999-99", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "gerente@raizes.com", "Gerente Geral Seed", 0, "GERENTE", "8D969EEF6ECAD3C29A3A629280E686CF0C3F5D5A86AFF3CA12020C923ADC6C92" }
                });

            migrationBuilder.UpdateData(
                table: "Itens",
                keyColumn: "Id",
                keyValue: new Guid("11111111-2222-3333-4444-555555555555"),
                column: "QuantidadeEstoque",
                value: 50);

            migrationBuilder.UpdateData(
                table: "Itens",
                keyColumn: "Id",
                keyValue: new Guid("22222222-3333-4444-5555-666666666666"),
                column: "QuantidadeEstoque",
                value: 30);

            migrationBuilder.UpdateData(
                table: "Itens",
                keyColumn: "Id",
                keyValue: new Guid("33333333-4444-5555-6666-777777777777"),
                column: "QuantidadeEstoque",
                value: 2);

            migrationBuilder.CreateIndex(
                name: "IX_Clientes_Email",
                table: "Clientes",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Clientes_Email",
                table: "Clientes");

            migrationBuilder.DeleteData(
                table: "Clientes",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaaa-bbbb-cccc-dddd-111111111111"));

            migrationBuilder.DeleteData(
                table: "Clientes",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaaa-bbbb-cccc-dddd-222222222222"));

            migrationBuilder.DropColumn(
                name: "Desconto",
                table: "Pedidos");

            migrationBuilder.DropColumn(
                name: "FormaPagamento",
                table: "Pedidos");

            migrationBuilder.DropColumn(
                name: "QuantidadeEstoque",
                table: "Itens");

            migrationBuilder.DropColumn(
                name: "perfilUsuario",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "senhaHash",
                table: "Clientes");
        }
    }
}
