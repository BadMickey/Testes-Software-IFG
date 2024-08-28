using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Moq;

namespace Leilao
{
    public class LeilaoServiceTests
    {
        private readonly Mock<IEmailService> _emailServiceMock;
        private readonly ILeilaoRepository _leilaoRepository;
        private readonly LeilaoService _leilaoService;
        private readonly IParticipanteRepository _participanteRepository;

        public LeilaoServiceTests()
        {
            // Repositório em memória para testes
            _emailServiceMock = new Mock<IEmailService>();
            _leilaoRepository = new InMemoryLeilaoRepository();
            _participanteRepository = new InMemoryParticipanteRepository();
            _leilaoService = new LeilaoService(_leilaoRepository, _emailServiceMock.Object, _participanteRepository);
        }

        [Fact]
        public void Deve_Criar_Leilao_Com_Status_Inativo()
        {
            var leilao = new Leilao("Leilão Teste", DateTime.Now.AddDays(5), 100);
            _leilaoService.CriarLeilao(leilao);

            var leilaoObtido = _leilaoRepository.ObterLeilaoPorId(leilao.Id);
            Assert.Equal(EstadoLeilao.INATIVO, leilaoObtido.Status); //Testando se o leilão irá ser criado apenas com status inativo
        }

        [Fact]
        public void Deve_Abrir_Leilao_Com_Status_Aberto()
        {
            var leilao = new Leilao("Leilão Teste", DateTime.Now.AddDays(5), 100);
            _leilaoService.CriarLeilao(leilao);

            _leilaoService.AbrirLeilao(leilao.Id);

            var leilaoObtido = _leilaoRepository.ObterLeilaoPorId(leilao.Id);
            Assert.Equal(EstadoLeilao.ABERTO, leilaoObtido.Status); //Testando se o leilão irá ser aberto com status aberto
        }

        [Fact]
        public void Deve_Expirar_Leilao_Quando_Data_Expirada()
        {
            var leilao = new Leilao("Leilão Teste", DateTime.Now.AddSeconds(-1), 100); // Expirado
            _leilaoService.CriarLeilao(leilao);

            _leilaoService.ExpirarLeilao(leilao.Id);

            var leilaoObtido = _leilaoRepository.ObterLeilaoPorId(leilao.Id);
            Assert.Equal(EstadoLeilao.EXPIRADO, leilaoObtido.Status); //Teste para expirar o leilão quando a data tiver expirada
        }

        [Fact]
        public void Nao_Deve_Expirar_Leilao_Sem_Acabar_Prazo()
        {
            var leilao = new Leilao("Leilão Teste", DateTime.Now.AddDays(5), 100);
            _leilaoService.CriarLeilao(leilao);
            _leilaoService.AbrirLeilao(leilao.Id);

            var leilaoObtido = _leilaoRepository.ObterLeilaoPorId(leilao.Id);
            Assert.Throws<InvalidOperationException>(() =>
                _leilaoService.ExpirarLeilao(leilao.Id)); //Teste para não expirar o leilão sem atingir a data de expiração
        }

        [Fact]
        public void Deve_Finalizar_Leilao_Expirado()
        {
            var leilao = new Leilao("Leilão Teste", DateTime.Now.AddSeconds(-1), 100);
            _leilaoService.CriarLeilao(leilao);
            _leilaoService.AbrirLeilao(leilao.Id);
            _leilaoService.ExpirarLeilao(leilao.Id);

            _leilaoService.FinalizarLeilao(leilao.Id);

            var leilaoObtido = _leilaoRepository.ObterLeilaoPorId(leilao.Id);
            Assert.Equal(EstadoLeilao.FINALIZADO, leilaoObtido.Status); //Teste para finalizar o leilão expirado
        }

        [Fact]
        public void Deve_Finalizar_Leilao_Aberto()
        {
            var leilao = new Leilao("Leilão Teste", DateTime.Now.AddDays(5), 100);
            _leilaoService.CriarLeilao(leilao);
            _leilaoService.AbrirLeilao(leilao.Id);

            _leilaoService.FinalizarLeilao(leilao.Id);

            var leilaoObtido = _leilaoRepository.ObterLeilaoPorId(leilao.Id);
            Assert.Equal(EstadoLeilao.FINALIZADO, leilaoObtido.Status);
        }

        [Fact]
        public void Não_Deve_Finalizar_Leilao_Inativo()
        {
            var leilao = new Leilao("Leilão Teste", DateTime.Now.AddDays(5), 100);
            _leilaoService.CriarLeilao(leilao);

            var leilaoObtido = _leilaoRepository.ObterLeilaoPorId(leilao.Id);
            Assert.Throws<InvalidOperationException>(() =>
                _leilaoService.FinalizarLeilao(leilao.Id));
        }

        [Fact]
        public void Deve_Adicionar_Lance_Com_Sucesso()
        {
            var leilao = new Leilao("Leilão Teste", DateTime.Now.AddDays(5), 100);
            var participante1 = new Participante("João", "joao@email.com");
            var participante2 = new Participante("Maria", "maria@email.com");

            _participanteRepository.AdicionarParticipante(participante1);
            _participanteRepository.AdicionarParticipante(participante2);

            _leilaoService.CriarLeilao(leilao);
            _leilaoService.AbrirLeilao(leilao.Id);
            _leilaoService.AdicionarLance(leilao.Id, participante1, 150);
            _leilaoService.AdicionarLance(leilao.Id, participante2, 200);

            var lances = _leilaoService.ObterLances(leilao.Id);

            Assert.Equal(2, lances.Count);
            Assert.Equal(200, lances[1].Valor);
        }

        [Fact]
        public void Nao_Deve_Aceitar_Lance_Leilao_Fechado()
        {
            var leilao = new Leilao("Leilão Teste", DateTime.Now.AddDays(5), 100);
            var participante = new Participante("João", "joao@email.com");

            _participanteRepository.AdicionarParticipante(participante);

            _leilaoService.CriarLeilao(leilao);

            Assert.Throws<InvalidOperationException>(() =>
                _leilaoService.AdicionarLance(leilao.Id, participante, 150));
        }

        [Fact]
        public void Nao_Deve_Aceitar_Lance_Abaixo_Do_Minimo()
        {
            var leilao = new Leilao("Leilão Teste", DateTime.Now.AddDays(5), 100);
            var participante = new Participante("João", "joao@email.com");

            _participanteRepository.AdicionarParticipante(participante);

            _leilaoService.CriarLeilao(leilao);
            _leilaoService.AbrirLeilao(leilao.Id);

            Assert.Throws<InvalidOperationException>(() =>
                _leilaoService.AdicionarLance(leilao.Id, participante, 50)); // Valor abaixo do mínimo
        }

        [Fact]
        public void Nao_Deve_Aceitar_Lance_Sequencial_Do_Mesmo_Participante()
        {
            var leilao = new Leilao("Leilão Teste", DateTime.Now.AddDays(5), 100);
            var participante = new Participante("João", "joao@email.com");

            _participanteRepository.AdicionarParticipante(participante);

            _leilaoService.CriarLeilao(leilao);
            _leilaoService.AbrirLeilao(leilao.Id);
            _leilaoService.AdicionarLance(leilao.Id, participante, 150);

            Assert.Throws<InvalidOperationException>(() =>
                _leilaoService.AdicionarLance(leilao.Id, participante, 200)); // Mesmo participante
        }

        [Fact]
        public void Nao_Deve_Aceitar_Lance_Menor_Igual_Anterior()
        {
            var leilao = new Leilao("Leilão Teste", DateTime.Now.AddDays(5), 100);
            var participante = new Participante("João", "joao@email.com");
            var participante2 = new Participante("Maria", "maria@email.com");
            
            _participanteRepository.AdicionarParticipante(participante);
            _participanteRepository.AdicionarParticipante(participante2);

            _leilaoService.CriarLeilao(leilao);
            _leilaoService.AbrirLeilao(leilao.Id);
            _leilaoService.AdicionarLance(leilao.Id, participante, 150);


            Assert.Throws<InvalidOperationException>(() =>
                _leilaoService.AdicionarLance(leilao.Id, participante2, 150)); // Mesmo lance anterior
        }

        [Fact]
        public void Deve_Enviar_Email_Ganhador()
        {
            var leilao = new Leilao("Leilão Teste", DateTime.Now.AddDays(5), 100);
            var participante1 = new Participante("João", "joao@email.com");
            var participante2 = new Participante("Maria", "maria@email.com");

            _participanteRepository.AdicionarParticipante(participante1);
            _participanteRepository.AdicionarParticipante(participante2);

            _leilaoService.CriarLeilao(leilao);
            _leilaoService.AbrirLeilao(leilao.Id);
            _leilaoService.AdicionarLance(leilao.Id, participante1, 150);
            _leilaoService.AdicionarLance(leilao.Id, participante2, 200);//maior lance
            _leilaoService.FinalizarLeilao(leilao.Id);

            _emailServiceMock.Verify(
                es => es.EnviarEmail(participante1.Email, "Parabéns!", $"Você venceu o leilão '{leilao.Titulo}'!"),
                Times.Once
            );
        }

        [Fact]
        public void Nao_Deve_Enviar_Email_Perdedor()
        {
            var leilao = new Leilao("Leilão Teste", DateTime.Now.AddDays(5), 100);
            var participante1 = new Participante("João", "joao@email.com");
            var participante2 = new Participante("Maria", "maria@email.com");

            _participanteRepository.AdicionarParticipante(participante1);
            _participanteRepository.AdicionarParticipante(participante2);

            _leilaoService.CriarLeilao(leilao);
            _leilaoService.AbrirLeilao(leilao.Id);
            _leilaoService.AdicionarLance(leilao.Id, participante1, 150);
            _leilaoService.AdicionarLance(leilao.Id, participante2, 200);
            _leilaoService.FinalizarLeilao(leilao.Id);

            _emailServiceMock.Verify(
                es => es.EnviarEmail(
                    participante1.Email,
                    It.IsAny<string>(),
                    It.IsAny<string>()
                ),
                Times.Never
            );
        }

        [Fact]
        public void Deve_Ganhador_Ter_MaiorLance()
        {
            var leilao = new Leilao("Leilão Teste", DateTime.Now.AddDays(5), 100);
            var participante1 = new Participante("João", "joao@email.com");
            var participante2 = new Participante("Maria", "maria@email.com");

            _participanteRepository.AdicionarParticipante(participante1);
            _participanteRepository.AdicionarParticipante(participante2);

            _leilaoService.CriarLeilao(leilao);
            _leilaoService.AbrirLeilao(leilao.Id);
            _leilaoService.AdicionarLance(leilao.Id, participante1, 150);
            _leilaoService.AdicionarLance(leilao.Id, participante2, 200);
            _leilaoService.FinalizarLeilao(leilao.Id);

            var maiorLance = _leilaoService.ObterMaiorLance(leilao.Id);
            var vencedorEsperado = maiorLance.Participante;

            _emailServiceMock.Verify(
            es => es.EnviarEmail(vencedorEsperado.Email, "Parabéns!", $"Você venceu o leilão '{leilao.Titulo}'!"),
                Times.Once
            );
        }
        [Fact]
        public void Deve_Avisar_Sem_Ganhador()
        {
            var leilao = new Leilao("Leilão Teste", DateTime.Now.AddDays(5), 100);

            _leilaoService.CriarLeilao(leilao);
            _leilaoService.AbrirLeilao(leilao.Id);

            var enviou = _leilaoService.FinalizarLeilao(leilao.Id);

            Assert.False(enviou);
        }
        [Fact]
        public void Nao_Deve_Adicionar_Lance_Participante_Nao_Cadastrado()
        {
            var leilao = new Leilao("Leilão Teste", DateTime.Now.AddDays(5), 100);
            var participante = new Participante("João", "joao@email.com");

            _leilaoService.CriarLeilao(leilao);
            _leilaoService.AbrirLeilao(leilao.Id);

            Assert.Throws<InvalidOperationException>(() =>
                _leilaoService.AdicionarLance(leilao.Id, participante, 150));
        }
        [Fact]
        public void Deve_Adicionar_Lance_Participante_Cadastrado()
        {
            // Arrange
            var leilao = new Leilao("Leilão Teste", DateTime.Now.AddDays(5), 100);
            var participanteCadastrado = new Participante("Maria", "maria@email.com");

            _participanteRepository.AdicionarParticipante(participanteCadastrado); // Cadastrando o participante

            _leilaoService.CriarLeilao(leilao);
            _leilaoService.AbrirLeilao(leilao.Id);
            _leilaoService.AdicionarLance(leilao.Id, participanteCadastrado, 150);

            // Assert
            var lances = _leilaoService.ObterLances(leilao.Id);
            Assert.Single(lances);
            Assert.Equal(150, lances[0].Valor);
        }
        [Fact]
        public void Deve_Retornar_Lances_Em_Ordem_Crescente()
        {
            var leilao = new Leilao("Leilão Teste", DateTime.Now.AddDays(5), 100);
            var participante1 = new Participante("João", "joao@email.com");
            var participante2 = new Participante("Maria", "maria@email.com");

            _participanteRepository.AdicionarParticipante(participante1);
            _participanteRepository.AdicionarParticipante(participante2);

            _leilaoService.CriarLeilao(leilao);
            _leilaoService.AbrirLeilao(leilao.Id);
            _leilaoService.AdicionarLance(leilao.Id, participante1, 150);
            _leilaoService.AdicionarLance(leilao.Id, participante2, 200);

            var lancesOrdenados = _leilaoService.ObterLances(leilao.Id);
            foreach (var lance in lancesOrdenados)
            {
                Console.WriteLine($"Participante: {lance.Participante.Nome}, Valor: {lance.Valor}, Data: {lance.Data}");
            }

            Assert.Equal(150, lancesOrdenados[0].Valor);
            Assert.Equal(200, lancesOrdenados[1].Valor);
        }

        [Fact]
        public void Deve_Retornar_Maior_Lance()
        {
            var leilao = new Leilao("Leilão Teste", DateTime.Now.AddDays(5), 100);
            var participante1 = new Participante("João", "joao@email.com");
            var participante2 = new Participante("Maria", "maria@email.com");

            _participanteRepository.AdicionarParticipante(participante1);
            _participanteRepository.AdicionarParticipante(participante2);

            _leilaoService.CriarLeilao(leilao);
            _leilaoService.AbrirLeilao(leilao.Id);
            _leilaoService.AdicionarLance(leilao.Id, participante1, 150);
            _leilaoService.AdicionarLance(leilao.Id, participante2, 200);

            var maiorLance = _leilaoService.ObterMaiorLance(leilao.Id);

            Assert.Equal(200, maiorLance.Valor);
        }        
        [Fact]
        public void Deve_Retornar_Menor_Lance()
        {
            var leilao = new Leilao("Leilão Teste", DateTime.Now.AddDays(5), 100);
            var participante1 = new Participante("João", "joao@email.com");
            var participante2 = new Participante("Maria", "maria@email.com");

            _participanteRepository.AdicionarParticipante(participante1);
            _participanteRepository.AdicionarParticipante(participante2);

            _leilaoService.CriarLeilao(leilao);
            _leilaoService.AbrirLeilao(leilao.Id);
            _leilaoService.AdicionarLance(leilao.Id, participante1, 150);
            _leilaoService.AdicionarLance(leilao.Id, participante2, 200);

            var menorLance = _leilaoService.ObterMenorLance(leilao.Id);

            Assert.Equal(150, menorLance.Valor);
        }
    }

    // Repositório em memória para facilitar os testes
    public class InMemoryLeilaoRepository : ILeilaoRepository
    {
        private readonly List<Leilao> _leiloes = new();

        public void AdicionarLeilao(Leilao leilao) => _leiloes.Add(leilao);

        public Leilao ObterLeilaoPorId(Guid id) => _leiloes.FirstOrDefault(l => l.Id == id);

        public void AtualizarLeilao(Leilao leilao)
        {
            var index = _leiloes.FindIndex(l => l.Id == leilao.Id);
            if (index >= 0)
            {
                _leiloes[index] = leilao;
            }
        }

        public List<Leilao> ListarLeiloes(EstadoLeilao? status)
        {
            return status.HasValue
                ? _leiloes.Where(l => l.Status == status).ToList()
                : _leiloes.ToList();
        }
    }
    public class InMemoryParticipanteRepository : IParticipanteRepository
    {
        private readonly List<Participante> _participantes = new();
        public Participante ObterParticipantePorId(Guid participanteId)
        {
            return _participantes.FirstOrDefault(p => p.Id == participanteId);
        }

        public void AdicionarParticipante(Participante participante)
        {
            _participantes.Add(participante);
        }
    }
}
