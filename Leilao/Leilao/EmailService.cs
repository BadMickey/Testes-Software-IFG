using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leilao
{
    public class EmailService : IEmailService
    {
        public void EnviarEmail(string destinatario, string assunto, string mensagem) 
        {
            Console.WriteLine($"Enviando email para {destinatario}: {assunto} - {mensagem}");
        }
    }
}
