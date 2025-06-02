using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SistemaRH.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaRH.Infra
{
    public class SqlContext : DbContext
    {
        public DbSet<Inscricao> Inscricoes { get; set; }
        public DbSet<ProcessoSeletivo> Processos { get; set; }
        public DbSet<Vaga> Vagas { get; set; }
        public SqlContext(DbContextOptions<SqlContext> options) : base(options) { }



        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Relacionamento Inscricao <-> Vaga (Muitos para Muitos)
            modelBuilder.Entity<Inscricao>()
                .HasMany(i => i.VagasParticipando)
                .WithMany(v => v.InscricoesParticipando)
                .UsingEntity<Dictionary<string, object>>(
                    "InscricaoVagas",
                    j => j.HasOne<Vaga>().WithMany().HasForeignKey("VagaId"),
                    j => j.HasOne<Inscricao>().WithMany().HasForeignKey("InscricaoId")
                );

            // Relacionamento Inscricao <-> ProcessoSeletivo (Muitos para Muitos)
            modelBuilder.Entity<Inscricao>()
                .HasMany(i => i.ProcessosParticipando)
                .WithMany(p => p.InscricoesParticipando)
                .UsingEntity<Dictionary<string, object>>(
                    "InscricaoProcessos",
                    j => j.HasOne<ProcessoSeletivo>().WithMany().HasForeignKey("ProcessoSeletivoId"),
                    j => j.HasOne<Inscricao>().WithMany().HasForeignKey("InscricaoId")
                );
        }
    }
}
