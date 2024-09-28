using Microsoft.EntityFrameworkCore;

namespace Leilao
{
    public class LeilaoDbContext : DbContext
    {
        public DbSet<Leilao> Leiloes { get; set; }
        public DbSet<Participante> Participantes { get; set; }
        public DbSet<Lance> Lances { get; set; }

        public LeilaoDbContext(DbContextOptions<LeilaoDbContext> options) : base(options) { }
        public LeilaoDbContext() { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Leilao>()
                .HasMany(l => l.Lances)
                .WithOne(l => l.Leilao)
                .HasForeignKey(l => l.LeilaoId);

            modelBuilder.Entity<Leilao>()
                .HasMany(l => l.Participantes)
                .WithMany(p => p.Leiloes)
                .UsingEntity<Dictionary<string, object>>(
                    "LeilaoParticipante",
                    j => j.HasOne<Participante>().WithMany().HasForeignKey("ParticipanteId"),
                    j => j.HasOne<Leilao>().WithMany().HasForeignKey("LeilaoId")
                );

            modelBuilder.Entity<Participante>()
                .HasMany(p => p.Lances)
                .WithOne(l => l.Participante)
                .HasForeignKey(l => l.ParticipanteId);

            // Configuração das colunas DateTime
            modelBuilder.Entity<Leilao>(entity =>
            {
                entity.Property(e => e.DataInicio)
                      .HasColumnType("timestamp without time zone");

                entity.Property(e => e.DataExpiracao)
                      .HasColumnType("timestamp without time zone");
            });

            modelBuilder.Entity<Lance>(entity =>
            {
                entity.Property(e => e.Data)
                      .HasColumnType("timestamp without time zone");
            });
        }
    }
}