using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Leilao
{
    public class EfLeilaoRepository : ILeilaoRepository
    {
        private readonly LeilaoDbContext _dbContext;

        public EfLeilaoRepository(LeilaoDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task<Participante> ObterParticipantePorIdAsync(Guid id)
        {
            return await _dbContext.Participantes.FirstOrDefaultAsync(p => p.Id == id);
        }
        public async Task AdicionarLeilao(Leilao leilao)
        {
            await _dbContext.Leiloes.AddAsync(leilao);
            await _dbContext.SaveChangesAsync();
        }
        public async Task AdicionarLance(Lance lance)
        {
            await _dbContext.Lances.AddAsync(lance);
            await _dbContext.SaveChangesAsync();
        }
        public async Task AdicionarParticipante(Participante participante)
        {
            var participanteExistente = await _dbContext.Participantes.FindAsync(participante.Id);
             if (participanteExistente == null)
            {
                await _dbContext.Participantes.AddAsync(participante);
                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task<Leilao> ObterLeilaoPorIdAsync(Guid id)
        {
            return await _dbContext.Leiloes
                .Include(l => l.Lances)
                .Include(l => l.Participantes)
                .FirstOrDefaultAsync(l => l.Id == id);
        }

        public async Task AtualizarLeilao(Leilao leilao)
        {
  
            _dbContext.Leiloes.Update(leilao);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<List<Leilao>> ListarLeiloes(EstadoLeilao? status)
        {
            return status.HasValue
                ? await _dbContext.Leiloes.Where(l => l.Status == status).ToListAsync()
                : await _dbContext.Leiloes.ToListAsync();
        }
    }
}
