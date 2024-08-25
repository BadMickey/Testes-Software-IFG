using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leilao
{
    public interface IEmailService
    {
        void EnviarEmail(string destinatario, string assunto, string mensagem);

    }
}
