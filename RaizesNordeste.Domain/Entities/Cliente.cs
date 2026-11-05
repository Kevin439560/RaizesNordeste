using RaizesNordeste.Dominio.Enums;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace RaizesNordeste.Domain.Entities
{
    public class Cliente
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Nome { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Cpf { get; set; } = string.Empty;
        public string senhaHash { get; set; } = string.Empty;
        public PerfilUsuario perfilUsuario { get; set; } = PerfilUsuario.CLIENTE;
        public int Pontos { get; set; } = 0;
        public bool ConsenteTermosLgpd { get; set; }
        public bool Anonimo { get; set; } = false;
        public DateTime DataCriacao { get; set; } = DateTime.UtcNow;

        public static string GerarSenhaHash(string senha)
        {
            var dadosHash = SHA256.HashData(Encoding.UTF8.GetBytes(senha));
            return Convert.ToHexString(dadosHash);
        }

        public bool ValidarSenha(string senha) => senhaHash == GerarSenhaHash(senha);

        public void AnonimizarDados()
        {
            Nome = "ANONIMIZADO";
            Email = $"anonimo_{Id.ToString()[..8]}@raizes.local";
            Cpf = $"ANON-{Id.ToString()[..8]}";
            senhaHash = string.Empty;
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

        public decimal ResgatarPontos(int pontosResgate)
        {
            if(!ConsenteTermosLgpd || Anonimo)
            {
                throw new InvalidOperationException("O cliente não participa do programa de fidelidade.");
            }
            if(Pontos < pontosResgate)
            {
                throw new InvalidOperationException("O cliente não possui pontos suficientes para resgatar.");
            }

            Pontos -= pontosResgate;

            // 10 pontos valem 1 real de desconto
            return Math.Round(pontosResgate / 10m, 2);

        }
    }
}
