using Microsoft.EntityFrameworkCore;
using RaizesNordeste.Domain.Entities;
using RaizesNordeste.Dominio.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace RaizesNordeste.Infra.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Cliente> Clientes => Set<Cliente>();
        public DbSet<Unidade> Unidades => Set<Unidade>();
        public DbSet<Item> Itens => Set<Item>();
        public DbSet<Pedido> Pedidos => Set<Pedido>();
        public DbSet<ItemPedido> ItensPedido => Set<ItemPedido>();
        public DbSet<Ocorrencia> Ocorrencias => Set<Ocorrencia>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 1. Cliente
            modelBuilder.Entity<Cliente>(e =>
            {
                e.HasKey(c => c.Id);
                e.Property(c => c.Nome).HasMaxLength(150).IsRequired();
                e.Property(c => c.Email).HasMaxLength(150).IsRequired();
                e.Property(c => c.Cpf).HasMaxLength(14).IsRequired();
                e.Property(c => c.senhaHash).HasMaxLength(128).IsRequired();
                e.Property(c => c.perfilUsuario).HasConversion<string>().HasMaxLength(20);
                e.HasIndex(c => c.Cpf).IsUnique();
                e.HasIndex(c => c.Email).IsUnique();
            });

            // 2. Unidade e Relacionamento 1:N com Item
            modelBuilder.Entity<Unidade>(e =>
            {
                e.HasKey(u => u.Id);
                e.Property(u => u.Nome).HasMaxLength(100).IsRequired();
                e.Property(u => u.Cidade).HasMaxLength(80).IsRequired();

                e.HasMany(u => u.ItensCardapio)
                 .WithOne()
                 .HasForeignKey(i => i.IdUnidade)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            // 3. Item (Cardápio)
            modelBuilder.Entity<Item>(e =>
            {
                e.HasKey(i => i.Id);
                e.Property(i => i.Nome).HasMaxLength(100).IsRequired();
                e.Property(i => i.Descricao).HasMaxLength(255);
                e.Property(i => i.Preco).HasPrecision(10, 2);
            });

            // 4. Pedido e Relacionamentos
            modelBuilder.Entity<Pedido>(e =>
            {
                e.HasKey(p => p.Id);
                e.Property(p => p.Canal).HasConversion<string>().HasMaxLength(20);
                e.Property(p => p.Status).HasConversion<string>().HasMaxLength(30);
                e.Property(p => p.FormaPagamento).HasMaxLength(30);
                e.Property(p => p.Desconto).HasPrecision(10, 2);
                e.Property(p => p.ValorTotal).HasPrecision(10, 2);
                e.Property(p => p.IdTransacaoExterna).HasMaxLength(100);
                e.Property(p => p.MotivoCancelamento).HasMaxLength(255);

                // Composição: Pedido 1 -> N ItemPedido
                e.HasMany(p => p.Itens)
                 .WithOne()
                 .HasForeignKey(ip => ip.IdPedido)
                 .OnDelete(DeleteBehavior.Cascade);

                // Chave estrangeira para Cliente (0..1)
                e.HasOne<Cliente>()
                 .WithMany()
                 .HasForeignKey(p => p.IdCliente)
                 .IsRequired(false);

                // Chave estrangeira para Unidade (1)
                e.HasOne<Unidade>()
                 .WithMany()
                 .HasForeignKey(p => p.IdUnidade)
                 .IsRequired();
            });

            // 5. ItemPedido
            modelBuilder.Entity<ItemPedido>(e =>
            {
                e.HasKey(ip => ip.Id);
                e.Property(ip => ip.PrecoUnitario).HasPrecision(10, 2);

                e.HasOne<Item>()
                 .WithMany()
                 .HasForeignKey(ip => ip.IdItem);
            });

            // 6. Ocorrencia (Auditoria)
            modelBuilder.Entity<Ocorrencia>(e =>
            {
                e.HasKey(o => o.Id);
                e.Property(o => o.TipoAcao).HasMaxLength(50).IsRequired();
                e.Property(o => o.IdAfetado).HasMaxLength(50).IsRequired();
                e.Property(o => o.Ator).HasMaxLength(100).IsRequired();

                e.HasOne<Unidade>()
                 .WithMany()
                 .HasForeignKey(o => o.IdUnidade)
                 .IsRequired(false);
            });


// =======================Dados iniciais para teste rapido===========================

            var idUnidadeMatriz = Guid.Parse("99999999-8888-7777-6666-555555555555");
            var dataFixaSeed = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var senhaPadraoHash = Cliente.GerarSenhaHash("123456");

            modelBuilder.Entity<Unidade>().HasData(new Unidade
            {
                Id = idUnidadeMatriz,
                Nome = "Raízes do Nordeste - Filial Centro",
                Cidade = "Fortaleza",
                CozinhaCompleta = true,
                Ativo = true
            });

            modelBuilder.Entity<Item>().HasData(
                new Item
                {
                    Id = Guid.Parse("11111111-2222-3333-4444-555555555555"),
                    IdUnidade = idUnidadeMatriz,
                    Nome = "Baião de Dois Tradicional",
                    Descricao = "Arroz, feijão de corda, queijo coalho assado e carne de sol na nata.",
                    Preco = 34.90m,
                    QuantidadeEstoque = 50,
                    Disponivel = true,
                    ProdutoSazonal = false
                },
                new Item
                {
                    Id = Guid.Parse("22222222-3333-4444-5555-666666666666"),
                    IdUnidade = idUnidadeMatriz,
                    Nome = "Cuscuz Recheado Sertanejo",
                    Descricao = "Cuscuz de milho flocão recheado com carne seca desfiada e requeijão do norte.",
                    Preco = 22.50m,
                    QuantidadeEstoque = 30,
                    Disponivel = true,
                    ProdutoSazonal = false
                },
                new Item
                {
                    Id = Guid.Parse("33333333-4444-5555-6666-777777777777"),
                    IdUnidade = idUnidadeMatriz,
                    Nome = "Canjica Junina Especial",
                    Descricao = "Canjica de milho verde com coco e canela (Cardápio Sazonal São João).",
                    Preco = 14.00m,
                    QuantidadeEstoque = 2,
                    Disponivel = true,
                    ProdutoSazonal = true
                }
            );
            modelBuilder.Entity<Cliente>().HasData(
            new Cliente
            {
                Id = Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-111111111111"),
                Nome = "Cliente Teste Seed",
                Email = "cliente@raizes.com",
                Cpf = "111.111.111-11",
                senhaHash = senhaPadraoHash,
                perfilUsuario = PerfilUsuario.CLIENTE,
                Pontos = 100,
                ConsenteTermosLgpd = true,
                Anonimo = false,
                DataCriacao = dataFixaSeed
            },
            new Cliente
            {
                Id = Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-222222222222"),
                Nome = "Gerente Geral Seed",
                Email = "gerente@raizes.com",
                Cpf = "999.999.999-99",
                senhaHash = senhaPadraoHash,
                perfilUsuario = PerfilUsuario.GERENTE,
                Pontos = 0,
                ConsenteTermosLgpd = true,
                Anonimo = false,
                DataCriacao = dataFixaSeed
            }
        );
        }
    }
}
