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

        public void CriarLeilao(Leilao leilao) => _leilaoRepository.AdicionarLeilao(leilao);

        public void AbrirLeilao(Guid id)
        {
            var leilao = _leilaoRepository.ObterLeilaoPorId(id);
            leilao.AbrirLeilao();
            _leilaoRepository.AtualizarLeilao(leilao);
        }

        public void ExpirarLeilao(Guid id)
        {
            var leilao = _leilaoRepository.ObterLeilaoPorId(id);
            leilao.ExpirarLeilao();
            _leilaoRepository.AtualizarLeilao(leilao);
        }

        public Boolean? FinalizarLeilao(Guid id)
        {
            var leilao = _leilaoRepository.ObterLeilaoPorId(id);
            leilao.FinalizarLeilao();
            _leilaoRepository.AtualizarLeilao(leilao);

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

        public void AdicionarLance(Guid leilaoId, Participante participante, decimal valor)
        {
            var leilao = _leilaoRepository.ObterLeilaoPorId(leilaoId);
            leilao.AdicionarLance(participante, valor);
            _leilaoRepository.AtualizarLeilao(leilao);
        }

        public List<Lance> ObterLances(Guid leilaoId) => _leilaoRepository.ObterLeilaoPorId(leilaoId).ObterLancesOrdenados();
        public Lance ObterMaiorLance(Guid leilaoId) => _leilaoRepository.ObterLeilaoPorId(leilaoId).ObterMaiorLance();
        public Lance ObterMenorLance(Guid leilaoId) => _leilaoRepository.ObterLeilaoPorId(leilaoId).ObterMenorLance();
    }
}
