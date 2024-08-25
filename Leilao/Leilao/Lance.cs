using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Leilao
{
    public class Lance
    {
        public Guid Id { get; private set; }
        public Participante Participante { get; private set; }
        public decimal Valor { get; private set; }
        public DateTime Data { get; private set; }

        public Lance(Participante participante, decimal valor)
        {
            Id = Guid.NewGuid();
            Participante = participante;
            Valor = valor;
            Data = DateTime.Now;
        }
    }
}
