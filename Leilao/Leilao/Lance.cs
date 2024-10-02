using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Leilao
{
    public class Lance
    {
        [Key]
        public Guid Id { get; private set; }

        [ForeignKey("Participante")]
        public Guid ParticipanteId { get; private set; }
        public Participante Participante { get; private set; }

        [ForeignKey("Leilao")]
        public Guid LeilaoId { get; private set; }
        public Leilao Leilao {  get; private set; }

        [Column(TypeName ="decimal(18,2)")]
        public decimal Valor { get; private set; }
        public DateTime Data { get; private set; }

        public Lance(Participante participante, decimal valor, Guid leilaoid)
        {
            Id = Guid.NewGuid();
            Participante = participante;
            ParticipanteId = participante.Id;
            LeilaoId = leilaoid;
            Valor = valor;
            Data = DateTime.Now;
        }
        private Lance() { }
    }
}
