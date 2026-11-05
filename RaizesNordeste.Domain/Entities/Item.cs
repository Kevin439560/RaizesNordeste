using System;
using System.Collections.Generic;
using System.Text;

namespace RaizesNordeste.Domain.Entities
{
    public class Item
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid IdUnidade { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Descricao { get; set; } = string.Empty;
        public decimal Preco { get; set; }
        public int QuantidadeEstoque { get; set; } = 0;
        public bool Disponivel { get; set; } = true;
        public bool ProdutoSazonal { get; set; } = false;

        public bool ValidarDisponibilidade()
        {
            return Disponivel;
        }

        public void BaixarEstoque(int quantidade)
        {
            if (quantidade <= 0)
                throw new ArgumentException("A quantidade deve ser maior que zero.");

            if (!Disponivel || QuantidadeEstoque < quantidade)
                throw new InvalidOperationException($"Estoque insuficiente para o item '{Nome}'. Disponível: {QuantidadeEstoque}, Solicitado: {quantidade}.");

            QuantidadeEstoque -= quantidade;
            if (QuantidadeEstoque == 0)
                Disponivel = false;
        }

        public void MovimentarEstoque(int quantidadeDiferenca)
        {
            if (QuantidadeEstoque + quantidadeDiferenca < 0)
                throw new InvalidOperationException("O saldo de estoque não pode ficar negativo.");

            QuantidadeEstoque += quantidadeDiferenca;
            Disponivel = QuantidadeEstoque > 0;
        }
    }
}
