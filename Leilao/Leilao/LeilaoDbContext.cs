using Microsoft.EntityFrameworkCore;

namespace Leilao
{
    public class LeilaoDbContext : DbContext
    {
        public DbSet<Leilao> Leiloes { get; set; }
        public DbSet<Participante> Participantes { get; set; }
        public DbSet<Lance> Lances { get; set; }

        public LeilaoDbContext(DbContextOptions<LeilaoDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Leilao>()
                .HasMany(l => l.Lances)
                .WithOne(l => l.Leilao)
                .HasForeignKey(l => l.LeilaoId);

            modelBuilder.Entity<Leilao>()
                .HasMany(l => l.Participantes)
                .WithMany(p => p.Lances)
                .UsingEntity<Dictionary<string, object>>(
                    "LeilaoParticipante",
                    j => j.HasOne<Participante>().WithMany().HasForeignKey("ParticipanteId"),
                    j => j.HasOne<Leilao>().WithMany().HasForeignKey("LeilaoId")
                );

            modelBuilder.Entity<Participante>()
                .HasMany(p => p.Lances)
                .WithOne(l => l.Participante)
                .HasForeignKey(l => l.ParticipanteId);
        }
    }
}
