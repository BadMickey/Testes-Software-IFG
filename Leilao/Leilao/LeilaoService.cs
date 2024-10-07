using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leilao
{
    public class LeilaoService
    {
        private readonly ILeilaoRepository _leilaoRepository;
        private readonly IEmailService _emailService;

        public LeilaoService(ILeilaoRepository leilaoRepository, IEmailService emailService)
        {
            _leilaoRepository = leilaoRepository;
            _emailService = emailService;
        }

        public async Task CriarLeilao(Leilao leilao) => await _leilaoRepository.AdicionarLeilao(leilao);

        public async Task EditarLeilao(Leilao leilao)
        {
            var leilaoAnterior = await _leilaoRepository.ObterLeilaoPorIdAsync(leilao.Id);
            
            if (leilaoAnterior == null)
            {
                throw new ArgumentException("Leilão não encontrado.");
            }
            if (leilaoAnterior.Status != EstadoLeilao.INATIVO)
            {
                throw new InvalidOperationException("Somente leilões inativos podem ser editados.");
            }

            leilaoAnterior.Titulo = leilao.Titulo;
            leilaoAnterior.DataExpiracao = leilao.DataExpiracao;
            leilaoAnterior.LanceMinimo = leilao.LanceMinimo;

            await _leilaoRepository.AtualizarLeilao(leilaoAnterior);
        }

        public async Task AbrirLeilao(Guid id)
        {
            var leilao = await _leilaoRepository.ObterLeilaoPorIdAsync(id);
            leilao.AbrirLeilao();
            await _leilaoRepository.AtualizarLeilao(leilao);
        }

        public async Task ExpirarLeilao(Guid id)
        {
            var leilao = await _leilaoRepository.ObterLeilaoPorIdAsync(id);
            leilao.ExpirarLeilao();
            await _leilaoRepository.AtualizarLeilao(leilao);
        }

        public async Task<Boolean?> FinalizarLeilao(Guid id)
        {
            var leilao = await _leilaoRepository.ObterLeilaoPorIdAsync(id);
            leilao.FinalizarLeilao();
            await _leilaoRepository.AtualizarLeilao(leilao);

            var vencedor = leilao.ObterMaiorLance()?.Participante;
            // Simulação de envio de e-mail
            if (vencedor != null)
            {
                _emailService.EnviarEmail(vencedor.Email, "Parabéns!", $"Você venceu o leilão '{leilao.Titulo}'!");
                return true;
            }
            else
            {
                Console.WriteLine($"O leilao '{leilao.Titulo}' não teve ganhador!");
                return false;
            }
        }

        public async Task<List<Leilao>> ListarLeiloes(EstadoLeilao status)
        {
            List<Leilao> leiloes = await _leilaoRepository.ListarLeiloes(status);
            return leiloes;
        }

        public async Task<List<Leilao>> ListarTodosLeiloes()
        {
            List<Leilao> leiloes = await _leilaoRepository.ListarTodosLeiloes();
            return leiloes;
        }

        public async Task<Leilao> ObterLeilaoPorIdAsync(Guid leilaoid)
        {
            var leilaoObtido = await _leilaoRepository.ObterLeilaoPorIdAsync(leilaoid);
            return leilaoObtido;
        }

        public async Task AdicionarParticipanteAsync(Guid leilaoid, Participante participante)
        {
            var leilao = await _leilaoRepository.ObterLeilaoPorIdAsync(leilaoid);
            if (leilao == null) throw new ArgumentException("Leilão não encontrado.");

            await _leilaoRepository.AdicionarParticipante(participante);
            leilao.AdicionarParticipante(participante);
            await _leilaoRepository.AtualizarLeilao(leilao);
        }

        public async Task EditarParticipanteAsync(Participante participantealterado)
        {
            await _leilaoRepository.AtualizarParticipante(participantealterado);
        }

        public async Task<Participante> ObterParticipanteAsync(Guid participanteid)
        {
            var participante = await _leilaoRepository.ObterParticipantePorIdAsync(participanteid);
            return participante;
        }

        public async Task<List<Participante>> ObterListaParticipantesAsync()
        {
            List<Participante> participantes = await _leilaoRepository.ObterTodosParticipantesAsync();
            return participantes;
        }

        public async Task AdicionarLanceAsync(Guid leilaoId, Participante participante, decimal valor)
        {
            var leilao = await _leilaoRepository.ObterLeilaoPorIdAsync(leilaoId);
            if (leilao == null) throw new ArgumentException("Leilão não encontrado.");

            var participanteCadastrado = leilao.Participantes.FirstOrDefault(p => p.Id == participante.Id);

            if (participanteCadastrado == null)
            {
                throw new InvalidOperationException("Participante não cadastrado.");
            }

            if (leilao.Status != EstadoLeilao.ABERTO)
                throw new InvalidOperationException("O leilão não está aberto para receber lances.");

            if (valor < leilao.LanceMinimo)
                throw new InvalidOperationException("O valor do lance é inferior ao lance mínimo permitido.");

            if (leilao.Lances.Any() && valor <= leilao.Lances.Max(l => l.Valor))
                throw new InvalidOperationException("O valor do lance deve ser superior ao maior lance atual.");

            if (leilao.Lances.Any() && leilao.Lances.Last().Participante.Id == participante.Id)
                throw new InvalidOperationException("O mesmo participante não pode dar dois lances consecutivos.");

            var lance = new Lance(participante, valor, leilaoId);

            await _leilaoRepository.AdicionarLance(lance);
        }

        public async Task<Lance> ObterLanceporIdAsync(Guid participanteId, int valorDoLance)
        {
            return await _leilaoRepository.ObterLance(participanteId, valorDoLance);
        }

        public async Task<List<Lance>> ObterLancesAsync(Guid leilaoId)
        {
            var leilao = await _leilaoRepository.ObterLeilaoPorIdAsync(leilaoId);
            return leilao.ObterLancesOrdenados();
        }

        public async Task<Lance> ObterMaiorLanceAsync(Guid leilaoId)
        {
            var leilao = await _leilaoRepository.ObterLeilaoPorIdAsync(leilaoId);
            return leilao.ObterMaiorLance();
        }

        public async Task<Lance> ObterMenorLanceAsync(Guid leilaoId)
        {
            var leilao = await _leilaoRepository.ObterLeilaoPorIdAsync(leilaoId);
            return leilao.ObterMenorLance();
        }
    }
}
