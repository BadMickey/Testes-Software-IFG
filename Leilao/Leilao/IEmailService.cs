using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics.CodeAnalysis;


namespace Leilao
{
    public interface IEmailService
    {
        [ExcludeFromCodeCoverage]
        void EnviarEmail(string destinatario, string assunto, string mensagem);

    }
}
