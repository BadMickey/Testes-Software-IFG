using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leilao
{
    public class Participante
    {
        [Key]
        public Guid Id { get; private set; }

        [Required]
        [MaxLength(100)]
        public string Nome { get; set; }

        [Required]
        [MaxLength(100)]
        public string Email { get; set; }
        public List<Lance> Lances { get; private set; } = new List<Lance>();
        public List<Leilao> Leiloes { get; private set; } = new List<Leilao>();

        public Participante(string nome, string email)
        {
            Id = Guid.NewGuid();
            Nome = nome;
            Email = email;
        }
        
        private Participante() { }
    }
}
