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

        public LeilaoServiceTests()
        {
            // Repositório em memória para testes
            _emailServiceMock = new Mock<IEmailService>();
            _leilaoRepository = new InMemoryLeilaoRepository();
            _leilaoService = new LeilaoService(_leilaoRepository, _emailServiceMock.Object);
        }
        [Fact]
        public void Deve_Listar_Leilao_Com_Status_Aberto()
        {
            var leilao = new Leilao("Leilão Teste", DateTime.Now.AddDays(5), 100);
            var leilao2 = new Leilao("Leilão Teste 2", DateTime.Now.AddDays(5), 100);
            var leilao3 = new Leilao("Leilão Teste 3", DateTime.Now.AddDays(5), 100);
            var leilao4 = new Leilao("Leilão Teste 4", DateTime.Now.AddDays(5), 100);
            _leilaoService.CriarLeilao(leilao);
            _leilaoService.CriarLeilao(leilao2);
            _leilaoService.CriarLeilao(leilao3);
            _leilaoService.CriarLeilao(leilao4);
            _leilaoService.AbrirLeilao(leilao3.Id);
            _leilaoService.AbrirLeilao(leilao4.Id);

            var leiloesAbertos = _leilaoRepository.ListarLeiloes(EstadoLeilao.ABERTO);
            Assert.Equal(2, leiloesAbertos.Count); 
            Assert.Contains(leilao3, leiloesAbertos);
            Assert.Contains(leilao4, leiloesAbertos);//Testando se vai retornar a lista de leilões abertos
        }
        [Fact]
        public void Deve_Listar_Leilao_Com_Status_Finalizado()
        {
            var leilao = new Leilao("Leilão Teste", DateTime.Now.AddDays(5), 100);
            var leilao2 = new Leilao("Leilão Teste 2", DateTime.Now.AddDays(5), 100);
            var leilao3 = new Leilao("Leilão Teste 3", DateTime.Now.AddDays(5), 100);
            var leilao4 = new Leilao("Leilão Teste 4", DateTime.Now.AddDays(5), 100);
            _leilaoService.CriarLeilao(leilao);
            _leilaoService.CriarLeilao(leilao2);
            _leilaoService.CriarLeilao(leilao3);
            _leilaoService.CriarLeilao(leilao4);
            _leilaoService.AbrirLeilao(leilao3.Id);
            _leilaoService.AbrirLeilao(leilao4.Id);
            _leilaoService.FinalizarLeilao(leilao3.Id);
            _leilaoService.FinalizarLeilao(leilao4.Id);

            var leiloesFinalizados = _leilaoRepository.ListarLeiloes(EstadoLeilao.FINALIZADO);
            Assert.Equal(2, leiloesFinalizados.Count);
            Assert.Contains(leilao3, leiloesFinalizados);
            Assert.Contains(leilao4, leiloesFinalizados);//Testando se vai retornar a lista de leilões Finalizados
        }
        [Fact]
        public void Deve_Listar_Leilao_Com_Status_Expirado()
        {
            var leilao = new Leilao("Leilão Teste", DateTime.Now.AddDays(5), 100);
            var leilao2 = new Leilao("Leilão Teste 2", DateTime.Now.AddDays(5), 100);
            var leilao3 = new Leilao("Leilão Teste 3", DateTime.Now.AddSeconds(-1), 100);
            var leilao4 = new Leilao("Leilão Teste 4", DateTime.Now.AddSeconds(-1), 100);
            _leilaoService.CriarLeilao(leilao);
            _leilaoService.CriarLeilao(leilao2);
            _leilaoService.CriarLeilao(leilao3);
            _leilaoService.CriarLeilao(leilao4);
            _leilaoService.AbrirLeilao(leilao3.Id);
            _leilaoService.AbrirLeilao(leilao4.Id);
            _leilaoService.ExpirarLeilao(leilao3.Id);
            _leilaoService.ExpirarLeilao(leilao4.Id);

            var leiloesExpirados = _leilaoRepository.ListarLeiloes(EstadoLeilao.EXPIRADO);
            Assert.Equal(2, leiloesExpirados.Count);
            Assert.Contains(leilao3, leiloesExpirados);
            Assert.Contains(leilao4, leiloesExpirados);//Testando se vai retornar a lista de leilões Expirados
        }
        [Fact]
        public void Deve_Listar_Leilao_Com_Status_Inativo()
        {
            var leilao = new Leilao("Leilão Teste", DateTime.Now.AddDays(5), 100);
            var leilao2 = new Leilao("Leilão Teste 2", DateTime.Now.AddDays(5), 100);
            var leilao3 = new Leilao("Leilão Teste 3", DateTime.Now.AddDays(5), 100);
            _leilaoService.CriarLeilao(leilao);
            _leilaoService.CriarLeilao(leilao2);
            _leilaoService.CriarLeilao(leilao3);
            _leilaoService.AbrirLeilao(leilao3.Id);

            var leiloesInativos = _leilaoRepository.ListarLeiloes(EstadoLeilao.INATIVO);
            Assert.Equal(2, leiloesInativos.Count); 
            Assert.Contains(leilao, leiloesInativos);
            Assert.Contains(leilao2, leiloesInativos);//Testando se vai retornar a lista de leilões inativos
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

            _leilaoService.CriarLeilao(leilao);
            _leilaoService.AdicionarParticipante(leilao.Id, participante1);
            _leilaoService.AdicionarParticipante(leilao.Id, participante2);
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

            _leilaoService.CriarLeilao(leilao);
            _leilaoService.AdicionarParticipante(leilao.Id, participante);

            Assert.Throws<InvalidOperationException>(() =>
                _leilaoService.AdicionarLance(leilao.Id, participante, 150));
        }

        [Fact]
        public void Nao_Deve_Aceitar_Lance_Abaixo_Do_Minimo()
        {
            var leilao = new Leilao("Leilão Teste", DateTime.Now.AddDays(5), 100);
            var participante = new Participante("João", "joao@email.com");

            _leilaoService.CriarLeilao(leilao);
            _leilaoService.AdicionarParticipante(leilao.Id, participante);
            _leilaoService.AbrirLeilao(leilao.Id);

            Assert.Throws<InvalidOperationException>(() =>
                _leilaoService.AdicionarLance(leilao.Id, participante, 50)); // Valor abaixo do mínimo
        }

        [Fact]
        public void Nao_Deve_Aceitar_Lance_Sequencial_Do_Mesmo_Participante()
        {
            var leilao = new Leilao("Leilão Teste", DateTime.Now.AddDays(5), 100);
            var participante = new Participante("João", "joao@email.com");

            _leilaoService.CriarLeilao(leilao);
            _leilaoService.AdicionarParticipante(leilao.Id, participante);
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

            _leilaoService.CriarLeilao(leilao);
            _leilaoService.AdicionarParticipante(leilao.Id, participante);
            _leilaoService.AdicionarParticipante(leilao.Id, participante2);
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

            _leilaoService.CriarLeilao(leilao);
            _leilaoService.AdicionarParticipante(leilao.Id, participante1);
            _leilaoService.AdicionarParticipante(leilao.Id, participante2);
            _leilaoService.AbrirLeilao(leilao.Id);
            _leilaoService.AdicionarLance(leilao.Id, participante1, 150);
            _leilaoService.AdicionarLance(leilao.Id, participante2, 200);//maior lance
            _leilaoService.FinalizarLeilao(leilao.Id);

            _emailServiceMock.Verify(
                es => es.EnviarEmail(participante2.Email, "Parabéns!", $"Você venceu o leilão '{leilao.Titulo}'!"),
                Times.Once
            );
        }

        [Fact]
        public void Nao_Deve_Enviar_Email_Perdedor()
        {
            var leilao = new Leilao("Leilão Teste", DateTime.Now.AddDays(5), 100);
            var participante1 = new Participante("João", "joao@email.com");
            var participante2 = new Participante("Maria", "maria@email.com");

            _leilaoService.CriarLeilao(leilao);
            _leilaoService.AdicionarParticipante(leilao.Id, participante1);
            _leilaoService.AdicionarParticipante(leilao.Id, participante2);
            _leilaoService.AbrirLeilao(leilao.Id);
            _leilaoService.AdicionarLance(leilao.Id, participante1, 150);
            _leilaoService.AdicionarLance(leilao.Id, participante2, 200);
            _leilaoService.FinalizarLeilao(leilao.Id);

            _emailServiceMock.Verify( //Testa se o participante perdedor não recebeu email
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

            _leilaoService.CriarLeilao(leilao);
            _leilaoService.AdicionarParticipante(leilao.Id, participante1);
            _leilaoService.AdicionarParticipante(leilao.Id, participante2);
            _leilaoService.AbrirLeilao(leilao.Id);
            _leilaoService.AdicionarLance(leilao.Id, participante1, 150);
            _leilaoService.AdicionarLance(leilao.Id, participante2, 200);
            _leilaoService.FinalizarLeilao(leilao.Id);

            var maiorLance = _leilaoService.ObterMaiorLance(leilao.Id);
            var vencedorEsperado = maiorLance.Participante;// Pega o participante com maior lance

            _emailServiceMock.Verify(
            es => es.EnviarEmail(vencedorEsperado.Email, "Parabéns!", $"Você venceu o leilão '{leilao.Titulo}'!"), //Testa se o participante esperado é o mesmo que ganhou
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
            var leilao = new Leilao("Leilão Teste", DateTime.Now.AddDays(5), 100);
            var participante = new Participante("Maria", "maria@email.com");

            _leilaoService.CriarLeilao(leilao);
            _leilaoService.AdicionarParticipante(leilao.Id, participante);
            _leilaoService.AbrirLeilao(leilao.Id);
            _leilaoService.AdicionarLance(leilao.Id, participante, 150);

            var lances = _leilaoService.ObterLances(leilao.Id);
            Assert.Single(lances);// Testa se o participante efetuou o lance
            Assert.Equal(150, lances[0].Valor);
        }
        [Fact]
        public void Deve_Retornar_Lances_Em_Ordem_Crescente()
        {
            var leilao = new Leilao("Leilão Teste", DateTime.Now.AddDays(5), 100);
            var participante1 = new Participante("João", "joao@email.com");
            var participante2 = new Participante("Maria", "maria@email.com");

            _leilaoService.CriarLeilao(leilao);
            _leilaoService.AdicionarParticipante(leilao.Id, participante1);
            _leilaoService.AdicionarParticipante(leilao.Id, participante2);
            _leilaoService.AbrirLeilao(leilao.Id);
            _leilaoService.AdicionarLance(leilao.Id, participante1, 150);
            _leilaoService.AdicionarLance(leilao.Id, participante2, 200);

            var lancesOrdenados = _leilaoService.ObterLances(leilao.Id);
            foreach (var lance in lancesOrdenados)
            {
                Console.WriteLine($"Participante: {lance.Participante.Nome}, Valor: {lance.Valor}, Data: {lance.Data}");
            }

            Assert.Equal(150, lancesOrdenados[0].Valor);//Teste para retornar os lances ordenados
            Assert.Equal(200, lancesOrdenados[1].Valor);
        }

        [Fact]
        public void Deve_Retornar_Maior_Lance()
        {
            var leilao = new Leilao("Leilão Teste", DateTime.Now.AddDays(5), 100);
            var participante1 = new Participante("João", "joao@email.com");
            var participante2 = new Participante("Maria", "maria@email.com");

            _leilaoService.CriarLeilao(leilao);
            _leilaoService.AdicionarParticipante(leilao.Id, participante1);
            _leilaoService.AdicionarParticipante(leilao.Id, participante2);
            _leilaoService.AbrirLeilao(leilao.Id);
            _leilaoService.AdicionarLance(leilao.Id, participante1, 150);
            _leilaoService.AdicionarLance(leilao.Id, participante2, 200);

            var maiorLance = _leilaoService.ObterMaiorLance(leilao.Id);

            Assert.Equal(200, maiorLance.Valor);//Teste para retornar o maior lance
        }        
        [Fact]
        public void Deve_Retornar_Menor_Lance()
        {
            var leilao = new Leilao("Leilão Teste", DateTime.Now.AddDays(5), 100);
            var participante1 = new Participante("João", "joao@email.com");
            var participante2 = new Participante("Maria", "maria@email.com");

            _leilaoService.CriarLeilao(leilao);
            _leilaoService.AdicionarParticipante(leilao.Id, participante1);
            _leilaoService.AdicionarParticipante(leilao.Id, participante2);
            _leilaoService.AbrirLeilao(leilao.Id);
            _leilaoService.AdicionarLance(leilao.Id, participante1, 150);
            _leilaoService.AdicionarLance(leilao.Id, participante2, 200);

            var menorLance = _leilaoService.ObterMenorLance(leilao.Id);

            Assert.Equal(150, menorLance.Valor);//Teste para retornar o menor lance
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
}
