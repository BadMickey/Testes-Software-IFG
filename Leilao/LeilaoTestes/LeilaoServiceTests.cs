using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Leilao;

namespace Leilao
{
    public class LeilaoServiceTests : IDisposable
    {
        private readonly ServiceProvider _serviceProvider;
        private readonly LeilaoDbContext _dbContext;
        private readonly Mock<IEmailService> _emailServiceMock;
        private readonly ILeilaoRepository _leilaoRepository;
        private readonly LeilaoService _leilaoService;

        public LeilaoServiceTests()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddDbContext<LeilaoDbContext>(options =>
                options.UseNpgsql("Host=localhost;Database=Leilao;Username=postgres"));

            // Mock do serviço de email
            _emailServiceMock = new Mock<IEmailService>();
            serviceCollection.AddScoped<IEmailService>(provider => _emailServiceMock.Object);

            // Registrar o Repositório e o Serviço
            serviceCollection.AddScoped<ILeilaoRepository, EfLeilaoRepository>();
            serviceCollection.AddScoped<LeilaoService>();

            _serviceProvider = serviceCollection.BuildServiceProvider();

            _dbContext = _serviceProvider.GetRequiredService<LeilaoDbContext>();
            _dbContext.Database.EnsureCreated();

            _leilaoService = _serviceProvider.GetRequiredService<LeilaoService>();
            _leilaoRepository = _serviceProvider.GetRequiredService<ILeilaoRepository>();

        }
        public void Dispose()
        {
            _dbContext.Database.ExecuteSqlRaw("TRUNCATE TABLE \"Leiloes\" RESTART IDENTITY CASCADE;");
            _dbContext.Database.ExecuteSqlRaw("TRUNCATE TABLE \"Participantes\" RESTART IDENTITY CASCADE;");
            _dbContext.Database.ExecuteSqlRaw("TRUNCATE TABLE \"LeilaoParticipante\" RESTART IDENTITY CASCADE;");
            //_dbContext.Database.EnsureDeleted();
            _dbContext.Database.ExecuteSqlRaw("TRUNCATE TABLE \"Lances\" RESTART IDENTITY CASCADE;");
            _dbContext.Dispose();

            _serviceProvider.Dispose();
        }

        [Fact]
        public async Task Deve_Listar_Todos_Leiloes()
        {
            var leilao = new Leilao("Leilão Teste", DateTime.Now.AddDays(5), 100);
            var leilao2 = new Leilao("Leilão Teste 2", DateTime.Now.AddDays(5), 100);
            var leilao3 = new Leilao("Leilão Teste 3", DateTime.Now.AddDays(5), 100);
            var leilao4 = new Leilao("Leilão Teste 4", DateTime.Now.AddDays(5), 100);

            await _leilaoService.CriarLeilao(leilao);
            await _leilaoService.CriarLeilao(leilao2);
            await _leilaoService.CriarLeilao(leilao3);
            await _leilaoService.CriarLeilao(leilao4);
            await _leilaoService.AbrirLeilao(leilao3.Id);
            await _leilaoService.AbrirLeilao(leilao4.Id);

            var leiloesAbertos = await _leilaoService.ListarTodosLeiloes();
            Assert.Equal(4, leiloesAbertos.Count);
            Assert.Contains(leilao, leiloesAbertos);
            Assert.Contains(leilao2, leiloesAbertos);
            Assert.Contains(leilao3, leiloesAbertos);
            Assert.Contains(leilao4, leiloesAbertos);//Testando se vai retornar a lista de leilões abertos
        }

        [Fact]
        public async Task Deve_Listar_Leilao_Com_Status_Aberto()
        {
            var leilao = new Leilao("Leilão Teste", DateTime.Now.AddDays(5), 100);
            var leilao2 = new Leilao("Leilão Teste 2", DateTime.Now.AddDays(5), 100);
            var leilao3 = new Leilao("Leilão Teste 3", DateTime.Now.AddDays(5), 100);
            var leilao4 = new Leilao("Leilão Teste 4", DateTime.Now.AddDays(5), 100);

            await _leilaoService.CriarLeilao(leilao);
            await _leilaoService.CriarLeilao(leilao2);
            await _leilaoService.CriarLeilao(leilao3);
            await _leilaoService.CriarLeilao(leilao4);
            await _leilaoService.AbrirLeilao(leilao3.Id);
            await _leilaoService.AbrirLeilao(leilao4.Id);

            var leiloesAbertos = await _leilaoRepository.ListarLeiloes(EstadoLeilao.ABERTO);
            Assert.Equal(2, leiloesAbertos.Count);
            Assert.Contains(leilao3, leiloesAbertos);
            Assert.Contains(leilao4, leiloesAbertos);//Testando se vai retornar a lista de leilões abertos
        }

        [Fact]
        public async Task Deve_Listar_Leilao_Com_Status_Finalizado()
        {
            var leilao = new Leilao("Leilão Teste", DateTime.Now.AddDays(5), 100);
            var leilao2 = new Leilao("Leilão Teste 2", DateTime.Now.AddDays(5), 100);
            var leilao3 = new Leilao("Leilão Teste 3", DateTime.Now.AddDays(5), 100);
            var leilao4 = new Leilao("Leilão Teste 4", DateTime.Now.AddDays(5), 100);

            await _leilaoService.CriarLeilao(leilao);
            await _leilaoService.CriarLeilao(leilao2);
            await _leilaoService.CriarLeilao(leilao3);
            await _leilaoService.CriarLeilao(leilao4);
            await _leilaoService.AbrirLeilao(leilao3.Id);
            await _leilaoService.AbrirLeilao(leilao4.Id);
            await _leilaoService.FinalizarLeilao(leilao3.Id);
            await _leilaoService.FinalizarLeilao(leilao4.Id);

            var leiloesFinalizados = await _leilaoRepository.ListarLeiloes(EstadoLeilao.FINALIZADO);
            Assert.Equal(2, leiloesFinalizados.Count);
            Assert.Contains(leilao3, leiloesFinalizados);
            Assert.Contains(leilao4, leiloesFinalizados);//Testando se vai retornar a lista de leilões Finalizados
        }

        [Fact]
        public async Task Deve_Listar_Leilao_Com_Status_Expirado()
        {
            var leilao = new Leilao("Leilão Teste", DateTime.Now.AddDays(5), 100);
            var leilao2 = new Leilao("Leilão Teste 2", DateTime.Now.AddDays(5), 100);
            var leilao3 = new Leilao("Leilão Teste 3", DateTime.Now.AddSeconds(-1), 100);
            var leilao4 = new Leilao("Leilão Teste 4", DateTime.Now.AddSeconds(-1), 100);

            await _leilaoService.CriarLeilao(leilao);
            await _leilaoService.CriarLeilao(leilao2);
            await _leilaoService.CriarLeilao(leilao3);
            await _leilaoService.CriarLeilao(leilao4);
            await _leilaoService.AbrirLeilao(leilao3.Id);
            await _leilaoService.AbrirLeilao(leilao4.Id);
            await _leilaoService.ExpirarLeilao(leilao3.Id);
            await _leilaoService.ExpirarLeilao(leilao4.Id);

            var leiloesExpirados = await _leilaoRepository.ListarLeiloes(EstadoLeilao.EXPIRADO);
            Assert.Equal(2, leiloesExpirados.Count);
            Assert.Contains(leilao3, leiloesExpirados);
            Assert.Contains(leilao4, leiloesExpirados);//Testando se vai retornar a lista de leilões Expirados
        }

        [Fact]
        public async Task Deve_Listar_Leilao_Com_Status_Inativo()
        {
            var leilao = new Leilao("Leilão Teste", DateTime.Now.AddDays(5), 100);
            var leilao2 = new Leilao("Leilão Teste 2", DateTime.Now.AddDays(5), 100);
            var leilao3 = new Leilao("Leilão Teste 3", DateTime.Now.AddDays(5), 100);

            await _leilaoService.CriarLeilao(leilao);
            await _leilaoService.CriarLeilao(leilao2);
            await _leilaoService.CriarLeilao(leilao3);
            await _leilaoService.AbrirLeilao(leilao3.Id);

            var leiloesInativos = await _leilaoRepository.ListarLeiloes(EstadoLeilao.INATIVO);
            Assert.Equal(2, leiloesInativos.Count);
            Assert.Contains(leilao, leiloesInativos);
            Assert.Contains(leilao2, leiloesInativos);//Testando se vai retornar a lista de leilões inativos
        }

        [Fact]
        public async Task Deve_Obter_Leilao()
        {
            var leilao = new Leilao("Leilão Teste", DateTime.Now.AddDays(5), 100);
            var leilao2 = new Leilao("Leilão Teste 2", DateTime.Now.AddDays(5), 100);
            var leilao3 = new Leilao("Leilão Teste 3", DateTime.Now.AddSeconds(-1), 100);
            var leilao4 = new Leilao("Leilão Teste 4", DateTime.Now.AddSeconds(-1), 100);

            await _leilaoService.CriarLeilao(leilao);
            await _leilaoService.CriarLeilao(leilao2);
            await _leilaoService.CriarLeilao(leilao3);
            await _leilaoService.CriarLeilao(leilao4);

            var leilaoObtido = await _leilaoRepository.ObterLeilaoPorIdAsync(leilao.Id);

            Assert.NotNull(leilaoObtido);
        }

        [Fact]
        public async Task Nao_Deve_Editar_Leilao()
        {
            var leilao = new Leilao("Leilão Teste", DateTime.Now.AddDays(5), 100);
            var leilao2 = new Leilao("Leilão Teste 2", DateTime.Now.AddDays(5), 100);
            var leilao3 = new Leilao("Leilão Teste 3", DateTime.Now.AddDays(5), 100);

            await _leilaoService.CriarLeilao(leilao);
            await _leilaoService.CriarLeilao(leilao2);
            await _leilaoService.CriarLeilao(leilao3);
            await _leilaoService.AbrirLeilao(leilao3.Id);
            await _leilaoService.AbrirLeilao(leilao2.Id);

            //Editar leilao
            leilao2.Titulo = "Leilao teste modificado";
            leilao2.DataExpiracao = DateTime.Now.AddDays(2);
            leilao2.LanceMinimo = 200;

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _leilaoService.EditarLeilao(leilao2));
        }

        [Fact]
        public async Task Deve_Editar_Leilao()
        {
            var leilao = new Leilao("Leilão Teste", DateTime.Now.AddDays(5), 100);
            var leilao2 = new Leilao("Leilão Teste 2", DateTime.Now.AddDays(5), 100);
            var leilao3 = new Leilao("Leilão Teste 3", DateTime.Now.AddDays(5), 100);

            await _leilaoService.CriarLeilao(leilao);
            await _leilaoService.CriarLeilao(leilao2);
            await _leilaoService.CriarLeilao(leilao3);
            await _leilaoService.AbrirLeilao(leilao3.Id);
            await _leilaoService.AbrirLeilao(leilao2.Id);

            //Editar leilao
            leilao.Titulo = "Leilao teste modificado";
            leilao.DataExpiracao = DateTime.Now.AddDays(2);
            leilao.LanceMinimo = 200;
            await _leilaoService.EditarLeilao(leilao);

            var leilaoAlterado = await _leilaoRepository.ObterLeilaoPorIdAsync(leilao.Id);

            Assert.Equal("Leilao teste modificado", leilaoAlterado.Titulo);
            Assert.Equal(leilao.DataExpiracao, leilaoAlterado.DataExpiracao);
            Assert.Equal(200, leilaoAlterado.LanceMinimo);
        }

        [Fact]
        public async Task Deve_Criar_Leilao_Com_Status_Inativo()
        {
            var leilao = new Leilao("Leilão Teste", DateTime.Now.AddDays(5), 100);

            await _leilaoService.CriarLeilao(leilao);

            var leilaoObtido = await _leilaoRepository.ObterLeilaoPorIdAsync(leilao.Id);
            Assert.Equal(EstadoLeilao.INATIVO, leilaoObtido.Status); //Testando se o leilão irá ser criado apenas com status inativo
        }

        [Fact]
        public async Task Deve_Abrir_Leilao_Com_Status_Aberto()
        {
            var leilao = new Leilao("Leilão Teste", DateTime.Now.AddDays(5), 100);

            await _leilaoService.CriarLeilao(leilao);
            await _leilaoService.AbrirLeilao(leilao.Id);

            var leilaoObtido = await _leilaoRepository.ObterLeilaoPorIdAsync(leilao.Id);
            Assert.Equal(EstadoLeilao.ABERTO, leilaoObtido.Status); //Testando se o leilão irá ser aberto com status aberto
        }

        [Fact]
        public async Task Deve_Avisar_Leilao_Ja_Aberto()
        {
            var leilao = new Leilao("Leilão Teste", DateTime.Now.AddDays(5), 100);

            await _leilaoService.CriarLeilao(leilao);
            await _leilaoService.AbrirLeilao(leilao.Id);

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _leilaoService.AbrirLeilao(leilao.Id));
        }
        [Fact]
        public async Task Deve_Expirar_Leilao_Quando_Data_Expirada()
        {
            var leilao = new Leilao("Leilão Teste", DateTime.Now.AddSeconds(-1), 100); // Expirado

            await _leilaoService.CriarLeilao(leilao);
            await _leilaoService.ExpirarLeilao(leilao.Id);

            var leilaoObtido = await _leilaoRepository.ObterLeilaoPorIdAsync(leilao.Id);
            Assert.Equal(EstadoLeilao.EXPIRADO, leilaoObtido.Status); //Teste para expirar o leilão quando a data tiver expirada
        }

        [Fact]
        public async Task Nao_Deve_Expirar_Leilao_Sem_Acabar_Prazo()
        {
            var leilao = new Leilao("Leilão Teste", DateTime.Now.AddDays(5), 100);

            await _leilaoService.CriarLeilao(leilao);
            await _leilaoService.AbrirLeilao(leilao.Id);

            var leilaoObtido = await _leilaoRepository.ObterLeilaoPorIdAsync(leilao.Id);
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _leilaoService.ExpirarLeilao(leilao.Id)); //Teste para não expirar o leilão sem atingir a data de expiração
        }

        [Fact]
        public async Task Deve_Finalizar_Leilao_Expirado()
        {
            var leilao = new Leilao("Leilão Teste", DateTime.Now.AddSeconds(-1), 100);

            await _leilaoService.CriarLeilao(leilao);
            await _leilaoService.AbrirLeilao(leilao.Id);
            await _leilaoService.ExpirarLeilao(leilao.Id);
            await _leilaoService.FinalizarLeilao(leilao.Id);

            var leilaoObtido = await _leilaoRepository.ObterLeilaoPorIdAsync(leilao.Id);
            Assert.Equal(EstadoLeilao.FINALIZADO, leilaoObtido.Status); //Teste para finalizar o leilão expirado
        }

        [Fact]
        public async Task Deve_Finalizar_Leilao_Aberto()
        {
            var leilao = new Leilao("Leilão Teste", DateTime.Now.AddDays(5), 100);

            await _leilaoService.CriarLeilao(leilao);
            await _leilaoService.AbrirLeilao(leilao.Id);
            await _leilaoService.FinalizarLeilao(leilao.Id);

            var leilaoObtido = await _leilaoRepository.ObterLeilaoPorIdAsync(leilao.Id);
            Assert.Equal(EstadoLeilao.FINALIZADO, leilaoObtido.Status);
        }

        [Fact]
        public async Task Não_Deve_Finalizar_Leilao_Inativo()
        {
            var leilao = new Leilao("Leilão Teste", DateTime.Now.AddDays(5), 100);

            await _leilaoService.CriarLeilao(leilao);

            var leilaoObtido = await _leilaoRepository.ObterLeilaoPorIdAsync(leilao.Id);
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _leilaoService.FinalizarLeilao(leilao.Id));
        }

        [Fact]
        public async Task Deve_Adicionar_Lance_Com_Sucesso()
        {
            var leilao = new Leilao("Leilão Teste", DateTime.Now.AddDays(5), 100);
            var participante1 = new Participante("João", "joao@email.com");
            var participante2 = new Participante("Maria", "maria@email.com");

            await _leilaoService.CriarLeilao(leilao);
            await _leilaoService.AdicionarParticipanteAsync(leilao.Id, participante1);
            await _leilaoService.AdicionarParticipanteAsync(leilao.Id, participante2);
            await _leilaoService.AbrirLeilao(leilao.Id);
            await _leilaoService.AdicionarLanceAsync(leilao.Id, participante1, 150);
            await _leilaoService.AdicionarLanceAsync(leilao.Id, participante2, 200);

            var lances = await _leilaoService.ObterLancesAsync(leilao.Id);

            Assert.Equal(2, lances.Count);
            Assert.Equal(200, lances[1].Valor);
        }

        [Fact]
        public async Task Nao_Deve_Aceitar_Lance_Leilao_Fechado()
        {
            var leilao = new Leilao("Leilão Teste", DateTime.Now.AddDays(5), 100);
            var participante = new Participante("João", "joao@email.com");

            await _leilaoService.CriarLeilao(leilao);
            await _leilaoService.AdicionarParticipanteAsync(leilao.Id, participante);

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _leilaoService.AdicionarLanceAsync(leilao.Id, participante, 150));
        }

        [Fact]
        public async Task Nao_Deve_Aceitar_Lance_Abaixo_Do_Minimo()
        {
            var leilao = new Leilao("Leilão Teste", DateTime.Now.AddDays(5), 100);
            var participante = new Participante("João", "joao@email.com");

            await _leilaoService.CriarLeilao(leilao);
            await _leilaoService.AdicionarParticipanteAsync(leilao.Id, participante);
            await _leilaoService.AbrirLeilao(leilao.Id);

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _leilaoService.AdicionarLanceAsync(leilao.Id, participante, 50)); // Valor abaixo do mínimo
        }

        [Fact]
        public async Task Nao_Deve_Aceitar_Lance_Sequencial_Do_Mesmo_Participante()
        {
            var leilao = new Leilao("Leilão Teste", DateTime.Now.AddDays(5), 100);
            var participante = new Participante("João", "joao@email.com");

            await _leilaoService.CriarLeilao(leilao);
            await _leilaoService.AdicionarParticipanteAsync(leilao.Id, participante);
            await _leilaoService.AbrirLeilao(leilao.Id);
            await _leilaoService.AdicionarLanceAsync(leilao.Id, participante, 150);

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _leilaoService.AdicionarLanceAsync(leilao.Id, participante, 200)); // Mesmo participante
        }

        [Fact]
        public async Task Nao_Deve_Aceitar_Lance_Menor_Igual_Anterior()
        {
            var leilao = new Leilao("Leilão Teste", DateTime.Now.AddDays(5), 100);
            var participante = new Participante("João", "joao@email.com");
            var participante2 = new Participante("Maria", "maria@email.com");

            await _leilaoService.CriarLeilao(leilao);
            await _leilaoService.AdicionarParticipanteAsync(leilao.Id, participante);
            await _leilaoService.AdicionarParticipanteAsync(leilao.Id, participante2);
            await _leilaoService.AbrirLeilao(leilao.Id);
            await _leilaoService.AdicionarLanceAsync(leilao.Id, participante, 150);


            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _leilaoService.AdicionarLanceAsync(leilao.Id, participante2, 150)); // Mesmo lance anterior
        }

        [Fact]
        public async Task Deve_Enviar_Email_Ganhador()
        {
            var leilao = new Leilao("Leilão Teste", DateTime.Now.AddDays(5), 100);
            var participante1 = new Participante("João", "joao@email.com");
            var participante2 = new Participante("Maria", "maria@email.com");

            await _leilaoService.CriarLeilao(leilao);
            await _leilaoService.AdicionarParticipanteAsync(leilao.Id, participante1);
            await _leilaoService.AdicionarParticipanteAsync(leilao.Id, participante2);
            await _leilaoService.AbrirLeilao(leilao.Id);
            await _leilaoService.AdicionarLanceAsync(leilao.Id, participante1, 150);
            await _leilaoService.AdicionarLanceAsync(leilao.Id, participante2, 200);//maior lance
            await _leilaoService.FinalizarLeilao(leilao.Id);

            _emailServiceMock.Verify(
                es => es.EnviarEmail(participante2.Email, "Parabéns!", $"Você venceu o leilão '{leilao.Titulo}'!"),
                Times.Once
            );
        }

        [Fact]
        public async Task Nao_Deve_Enviar_Email_Perdedor()
        {
            var leilao = new Leilao("Leilão Teste", DateTime.Now.AddDays(5), 100);
            var participante1 = new Participante("João", "joao@email.com");
            var participante2 = new Participante("Maria", "maria@email.com");

            await _leilaoService.CriarLeilao(leilao);
            await _leilaoService.AdicionarParticipanteAsync(leilao.Id, participante1);
            await _leilaoService.AdicionarParticipanteAsync(leilao.Id, participante2);
            await _leilaoService.AbrirLeilao(leilao.Id);
            await _leilaoService.AdicionarLanceAsync(leilao.Id, participante1, 150);
            await _leilaoService.AdicionarLanceAsync(leilao.Id, participante2, 200);
            await _leilaoService.FinalizarLeilao(leilao.Id);

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
        public async Task Deve_Ganhador_Ter_MaiorLance()
        {
            var leilao = new Leilao("Leilão Teste", DateTime.Now.AddDays(5), 100);
            var participante1 = new Participante("João", "joao@email.com");
            var participante2 = new Participante("Maria", "maria@email.com");

            await _leilaoService.CriarLeilao(leilao);
            await _leilaoService.AdicionarParticipanteAsync(leilao.Id, participante1);
            await _leilaoService.AdicionarParticipanteAsync(leilao.Id, participante2);
            await _leilaoService.AbrirLeilao(leilao.Id);
            await _leilaoService.AdicionarLanceAsync(leilao.Id, participante1, 150);
            await _leilaoService.AdicionarLanceAsync(leilao.Id, participante2, 200);
            await _leilaoService.FinalizarLeilao(leilao.Id);

            var maiorLance = await _leilaoService.ObterMaiorLanceAsync(leilao.Id);
            var vencedorEsperado = maiorLance.Participante;// Pega o participante com maior lance

            _emailServiceMock.Verify(
            es => es.EnviarEmail(vencedorEsperado.Email, "Parabéns!", $"Você venceu o leilão '{leilao.Titulo}'!"), //Testa se o participante esperado é o mesmo que ganhou
                Times.Once
            );
        }
        [Fact]
        public async Task Deve_Avisar_Sem_Ganhador()
        {
            var leilao = new Leilao("Leilão Teste", DateTime.Now.AddDays(5), 100);

            await _leilaoService.CriarLeilao(leilao);
            await _leilaoService.AbrirLeilao(leilao.Id);

            var enviou = await _leilaoService.FinalizarLeilao(leilao.Id);

            Assert.False(enviou);
        }
        [Fact]
        public async Task Nao_Deve_Adicionar_Lance_Participante_Nao_Cadastrado()
        {
            var leilao = new Leilao("Leilão Teste", DateTime.Now.AddDays(5), 100);
            var participante = new Participante("João", "joao@email.com");

            await _leilaoService.CriarLeilao(leilao);
            await _leilaoService.AbrirLeilao(leilao.Id);

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _leilaoService.AdicionarLanceAsync(leilao.Id, participante, 150));
        }
        [Fact]
        public async Task Deve_Adicionar_Participante_Ao_Leilao()
        {
            var leilao = new Leilao("Leilão Teste", DateTime.Now.AddDays(5), 100);
            var participante = new Participante("Participante Teste", "teste@example.com");

            await _leilaoService.CriarLeilao(leilao);
            await _leilaoService.AdicionarParticipanteAsync(leilao.Id, participante);

            var leilaoAtualizado = await _leilaoRepository.ObterLeilaoPorIdAsync(leilao.Id);
            Assert.Contains(leilaoAtualizado.Participantes, p => p.Id == participante.Id);
        }

        [Fact]
        public async Task Deve_Editar_Participante()
        {
            var leilao = new Leilao("Leilão Teste", DateTime.Now.AddDays(5), 100);
            var participante = new Participante("Participante Teste", "teste@example.com");

            await _leilaoService.CriarLeilao(leilao);
            await _leilaoService.AdicionarParticipanteAsync(leilao.Id, participante);

            var participanteAntigo = await _leilaoService.ObterParticipanteAsync(participante.Id);
            var participanteAlterado = participanteAntigo;

            participanteAlterado.Email = "emailmudado@example.com";
            participanteAlterado.Nome = "Novo nome";

            await _leilaoService.EditarParticipanteAsync(participanteAlterado);
            var participanteAtualizado = await _leilaoService.ObterParticipanteAsync(participante.Id);


            Assert.True(participanteAlterado == participanteAtualizado);
        }

        [Fact]
        public async Task Deve_Obter_Participante()
        {
            var leilao = new Leilao("Leilão Teste", DateTime.Now.AddDays(5), 100);
            var participante = new Participante("Participante Teste", "teste@example.com");

            await _leilaoService.CriarLeilao(leilao);
            await _leilaoService.AdicionarParticipanteAsync(leilao.Id, participante);

            var participanteObtido = await _leilaoService.ObterParticipanteAsync(participante.Id);

            Assert.NotNull(participanteObtido);
        }

        [Fact]
        public async Task Deve_Retornar_Lista_Participantes()
        {
            var leilao = new Leilao("Leilão Teste", DateTime.Now.AddDays(5), 100);
            var participante = new Participante("Participante Teste", "teste@example.com");
            var participante2 = new Participante("Participante Teste2", "teste@example.com");
            var participante3 = new Participante("Participante Teste3", "teste@example.com");

            await _leilaoService.CriarLeilao(leilao);
            await _leilaoService.AdicionarParticipanteAsync(leilao.Id, participante);
            await _leilaoService.AdicionarParticipanteAsync(leilao.Id, participante2);
            await _leilaoService.AdicionarParticipanteAsync(leilao.Id, participante3);

            List<Participante> participantes = await _leilaoService.ObterListaParticipantesAsync();
            Assert.Equal(3, participantes.Count);
        }

        [Fact]
        public async Task Deve_Adicionar_Lance_Participante_Cadastrado()
        {
            var leilao = new Leilao("Leilão Teste", DateTime.Now.AddDays(5), 100);
            var participante = new Participante("Maria", "maria@email.com");

            await _leilaoService.CriarLeilao(leilao);
            await _leilaoService.AdicionarParticipanteAsync(leilao.Id, participante);
            await _leilaoService.AbrirLeilao(leilao.Id);
            await _leilaoService.AdicionarLanceAsync(leilao.Id, participante, 150);

            var lances = await _leilaoService.ObterLancesAsync(leilao.Id);
            Assert.Single(lances);// Testa se o participante efetuou o lance
            Assert.Equal(150, lances[0].Valor);
        }

        [Fact]
        public async Task Deve_Retornar_Lance()
        {
            var leilao = new Leilao("Leilão Teste", DateTime.Now.AddDays(5), 100);
            var participante = new Participante("Maria", "maria@email.com");
            var valorDoLance = 150;


            await _leilaoService.CriarLeilao(leilao);
            await _leilaoService.AdicionarParticipanteAsync(leilao.Id, participante);
            await _leilaoService.AbrirLeilao(leilao.Id);
            await _leilaoService.AdicionarLanceAsync(leilao.Id, participante, valorDoLance);

            var leilaoObtido = await _leilaoRepository.ObterLeilaoPorIdAsync(leilao.Id);
            var lance = await _leilaoService.ObterLanceporIdAsync(participante.Id, valorDoLance);


            Assert.True(lance.ParticipanteId == participante.Id && lance.Valor == valorDoLance);
        }

        [Fact]
        public async Task Deve_Retornar_Lances_Em_Ordem_Crescente()
        {
            var leilao = new Leilao("Leilão Teste", DateTime.Now.AddDays(5), 100);
            var participante1 = new Participante("João", "joao@email.com");
            var participante2 = new Participante("Maria", "maria@email.com");

            await _leilaoService.CriarLeilao(leilao);
            await _leilaoService.AdicionarParticipanteAsync(leilao.Id, participante1);
            await _leilaoService.AdicionarParticipanteAsync(leilao.Id, participante2);
            await _leilaoService.AbrirLeilao(leilao.Id);
            await _leilaoService.AdicionarLanceAsync(leilao.Id, participante1, 150);
            await _leilaoService.AdicionarLanceAsync(leilao.Id, participante2, 200);

            var lancesOrdenados = await _leilaoService.ObterLancesAsync(leilao.Id);
            foreach (var lance in lancesOrdenados)
            {
                Console.WriteLine($"Participante: {lance.Participante.Nome}, Valor: {lance.Valor}, Data: {lance.Data}");
            }

            Assert.Equal(150, lancesOrdenados[0].Valor);//Teste para retornar os lances ordenados
            Assert.Equal(200, lancesOrdenados[1].Valor);
        }

        [Fact]
        public async Task Deve_Retornar_Maior_Lance()
        {
            var leilao = new Leilao("Leilão Teste", DateTime.Now.AddDays(5), 100);
            var participante1 = new Participante("João", "joao@email.com");
            var participante2 = new Participante("Maria", "maria@email.com");

            await _leilaoService.CriarLeilao(leilao);
            await _leilaoService.AdicionarParticipanteAsync(leilao.Id, participante1);
            await _leilaoService.AdicionarParticipanteAsync(leilao.Id, participante2);
            await _leilaoService.AbrirLeilao(leilao.Id);
            await _leilaoService.AdicionarLanceAsync(leilao.Id, participante1, 150);
            await _leilaoService.AdicionarLanceAsync(leilao.Id, participante2, 200);

            var maiorLance = await _leilaoService.ObterMaiorLanceAsync(leilao.Id);

            Assert.Equal(200, maiorLance.Valor);//Teste para retornar o maior lance
        }

        [Fact]
        public async Task Deve_Retornar_Menor_Lance()
        {
            var leilao = new Leilao("Leilão Teste", DateTime.Now.AddDays(5), 100);
            var participante1 = new Participante("João", "joao@email.com");
            var participante2 = new Participante("Maria", "maria@email.com");

            await _leilaoService.CriarLeilao(leilao);
            await _leilaoService.AdicionarParticipanteAsync(leilao.Id, participante1);
            await _leilaoService.AdicionarParticipanteAsync(leilao.Id, participante2);
            await _leilaoService.AbrirLeilao(leilao.Id);
            await _leilaoService.AdicionarLanceAsync(leilao.Id, participante1, 150);
            await _leilaoService.AdicionarLanceAsync(leilao.Id, participante2, 200);

            var menorLance = await _leilaoService.ObterMenorLanceAsync(leilao.Id);

            Assert.Equal(150, menorLance.Valor);//Teste para retornar o menor lance
        }
    }
}
