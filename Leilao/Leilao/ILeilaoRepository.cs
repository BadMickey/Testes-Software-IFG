using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leilao
{
    public interface ILeilaoRepository
    {
        void AdicionarLeilao(Leilao leilao);
        Leilao ObterLeilaoPorId(Guid id);
        void AtualizarLeilao(Leilao leilao);
        List<Leilao> ListarLeiloes(EstadoLeilao? status);
    }
}
