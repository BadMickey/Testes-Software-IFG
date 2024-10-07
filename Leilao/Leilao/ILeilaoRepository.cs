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
        Task AdicionarLance(Lance lance);
        Task AdicionarParticipante(Participante participante);
        Task<Leilao> ObterLeilaoPorIdAsync(Guid id);
        Task AtualizarParticipante(Participante participante);
        Task<Lance> ObterLance(Guid id, int valor);
        Task<List<Participante>> ObterTodosParticipantesAsync();
        Task<Participante> ObterParticipantePorIdAsync(Guid id); // Novo método
        Task AtualizarLeilao(Leilao leilao);
        Task<List<Leilao>> ListarTodosLeiloes();
        Task<List<Leilao>> ListarLeiloes(EstadoLeilao? status);
    }
}
