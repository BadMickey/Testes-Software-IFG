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

        public async Task <Boolean?> FinalizarLeilao(Guid id)
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

            //leilao.AdicionarLance(participante, valor, leilao.Id);
            var lance = new Lance(participante, valor, leilaoId);

            await _leilaoRepository.AdicionarLance(lance);
        }
        public async Task AdicionarParticipanteAsync(Guid leilaoid, Participante participante)
        {
            var leilao = await _leilaoRepository.ObterLeilaoPorIdAsync(leilaoid);
            if (leilao == null) throw new ArgumentException("Leilão não encontrado.");
            
            await _leilaoRepository.AdicionarParticipante(participante);
            leilao.AdicionarParticipante(participante);
            await _leilaoRepository.AtualizarLeilao(leilao);
        }
        public async Task<Participante> ObterParticipanteAsync(Guid participanteid)
        {
            var participante = await _leilaoRepository.ObterParticipantePorIdAsync(participanteid);
            return participante;
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
