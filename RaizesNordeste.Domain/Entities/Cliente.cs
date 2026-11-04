using System;
using System.Collections.Generic;
using System.Text;

namespace RaizesNordeste.Domain.Entities
{
    public class Cliente
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Nome { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Cpf { get; set; } = string.Empty;
        public int Pontos { get; set; } = 0;
        public bool ConsenteTermosLgpd { get; set; }
        public bool Anonimo { get; set; } = false;
        public DateTime DataCriacao { get; set; } = DateTime.UtcNow;

        public void AnonimizarDados()
        {
            Nome = "ANONIMIZADO";
            Email = $"anonimo_{Id.ToString()[..8]}@raizes.local";
            Cpf = $"ANON-{Id.ToString()[..8]}";
            Pontos = 0;
            ConsenteTermosLgpd = false;
            Anonimo = true;
        }

        public void AdicionarPontos(decimal valorPedido)
        {
            if (ConsenteTermosLgpd && !Anonimo)
            {
                Pontos += (int)Math.Floor(valorPedido);
            }
        }
    }
}
