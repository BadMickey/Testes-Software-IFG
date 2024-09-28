using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leilao
{
    public interface ILeilaoRepository
    {
        Task AdicionarLeilao(Leilao leilao);
        Task <Leilao> ObterLeilaoPorIdAsync(Guid id);
        Task AtualizarLeilao(Leilao leilao);
        Task <List<Leilao>> ListarLeiloes(EstadoLeilao? status);
    }
}
