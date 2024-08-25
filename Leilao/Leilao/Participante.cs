using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leilao
{
    public class Participante
    {
        public Guid Id { get; private set; }
        public string Nome { get; set; }
        public string Email { get; set; }

        public Participante(string nome, string email)
        {
            Id = Guid.NewGuid();
            Nome = nome;
            Email = email;
        }
    }
}
