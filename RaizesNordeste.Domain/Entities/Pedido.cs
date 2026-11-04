using RaizesNordeste.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace RaizesNordeste.Domain.Entities
{
    public class Pedido
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid? IdCliente { get; set; }
        public Guid IdUnidade { get; set; }
        public CanalPedido Canal { get; set; }
        public StatusPedido Status { get; set; } = StatusPedido.AGUARDANDO_PAGAMENTO;
        public decimal ValorTotal { get; set; }
        public string? IdTransacaoExterna { get; set; }
        public string? MotivoCancelamento { get; set; }
        public DateTime DataCriacao { get; set; } = DateTime.UtcNow;

        public List<ItemPedido> Itens { get; set; } = new();

        public void CalcularValorTotal()
        {
            ValorTotal = Itens.Sum(i => i.CalcularSubtotal());
        }

        public void ConfirmarPagamento(string idTransacao)
        {
            IdTransacaoExterna = idTransacao;
            Status = StatusPedido.RECEBIDO;
        }

        public void AtualizarStatus(StatusPedido novoStatus)
        {
            if (Status == StatusPedido.CANCELADO)
                throw new InvalidOperationException("Não é possível alterar um pedido já cancelado.");

            Status = novoStatus;
        }

        public void Cancelar(string motivo)
        {
            if (string.IsNullOrWhiteSpace(motivo))
                throw new ArgumentException("A justificativa é obrigatória para o cancelamento.");

            Status = StatusPedido.CANCELADO;
            MotivoCancelamento = motivo;
        }
    }
}
