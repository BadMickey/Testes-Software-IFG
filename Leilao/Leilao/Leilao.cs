using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leilao
{
    public class Leilao
    {
        [Key]
        public Guid Id { get; private set; }

        [Required]
        [MaxLength(100)]
        public string Titulo { get; private set; }
        public DateTime? DataInicio { get; private set; }

        [Required]
        public DateTime DataExpiracao { get; private set; }

        [Required]
        public EstadoLeilao Status { get; private set; }
        public List<Lance> Lances { get; private set; } = new List<Lance>();
        public List<Participante> Participantes { get; private set; } = new List<Participante>();

        [Column(TypeName = "decimal(18,2)")]
        public decimal LanceMinimo { get; private set; }

        public Leilao(string titulo, DateTime dataExpiracao, decimal lanceMinimo)
        {
            Id = Guid.NewGuid();
            Titulo = titulo;
            DataExpiracao = dataExpiracao;
            LanceMinimo = lanceMinimo;
            Status = EstadoLeilao.INATIVO;
        }

        public void AbrirLeilao()
        {
            if (Status != EstadoLeilao.INATIVO)
                throw new InvalidOperationException("O leilão já foi aberto ou está em um estado inválido.");

            Status = EstadoLeilao.ABERTO;
            DataInicio = DateTime.Now;
        }

        public void ExpirarLeilao()
        {
            if (DateTime.Now >= DataExpiracao)
                Status = EstadoLeilao.EXPIRADO;
            else
                throw new InvalidOperationException("O leilão não chegou na data de expiração ainda!");
        }

        public void FinalizarLeilao()
        {
            if (Status == EstadoLeilao.ABERTO || Status == EstadoLeilao.EXPIRADO)
                Status = EstadoLeilao.FINALIZADO;
            else
                throw new InvalidOperationException("O leilão não foi aberto ou não está expirado para finalizar!");
        }

        public void AdicionarLance(Participante participante, decimal valor)
        {
            if (Status != EstadoLeilao.ABERTO)
                throw new InvalidOperationException("O leilão não está aberto para receber lances.");

            if (valor < LanceMinimo)
                throw new InvalidOperationException("O valor do lance é inferior ao lance mínimo permitido.");

            if (Lances.Any() && valor <= Lances.Max(l => l.Valor))
                throw new InvalidOperationException("O valor do lance deve ser superior ao maior lance atual.");

            if (Lances.Any() && Lances.Last().Participante.Id == participante.Id)
                throw new InvalidOperationException("O mesmo participante não pode dar dois lances consecutivos.");

            Lances.Add(new Lance(participante, valor));
        }
        public void AdicionarParticipante(Participante participante)
        {
            Participantes.Add(participante);
        }

        public Lance ObterMaiorLance() => Lances.OrderByDescending(l => l.Valor).FirstOrDefault();
        public Lance ObterMenorLance() => Lances.OrderBy(l => l.Valor).FirstOrDefault();
        public List<Lance> ObterLancesOrdenados() => Lances.OrderBy(l => l.Valor).ToList();

        private Leilao(){}
    }
}
