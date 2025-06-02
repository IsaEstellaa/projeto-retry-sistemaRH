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
        // tabelas do banco de dados
        public DbSet<Inscricao> Inscricoes { get; set; }
        public DbSet<ProcessoSeletivo> Processos { get; set; }
        public DbSet<Vaga> Vagas { get; set; }
        public SqlContext(DbContextOptions<SqlContext> options) : base(options) { }

        // configuração dos relacionamentos entre entidades
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // configura relacionamento muitos-para-muitos entre inscricao e vaga
            modelBuilder.Entity<Inscricao>()
                .HasMany(i => i.VagasParticipando)
                .WithMany(v => v.InscricoesParticipando)
                .UsingEntity<Dictionary<string, object>>(
                    "InscricaoVagas", // nome da tabela de junção
                    j => j.HasOne<Vaga>().WithMany().HasForeignKey("VagaId"), // chave estrangeira pra vaga
                    j => j.HasOne<Inscricao>().WithMany().HasForeignKey("InscricaoId") // chave estrangeira pra inscricao
                );

            // configura relacionamento muitos-para-muitos entre inscricao e processoSeletivo
            modelBuilder.Entity<Inscricao>()
                .HasMany(i => i.ProcessosParticipando)
                .WithMany(p => p.InscricoesParticipando)
                .UsingEntity<Dictionary<string, object>>(
                    "InscricaoProcessos", // nome da tabela de junção
                    j => j.HasOne<ProcessoSeletivo>().WithMany().HasForeignKey("ProcessoSeletivoId"), // chave estrangeira pra processo
                    j => j.HasOne<Inscricao>().WithMany().HasForeignKey("InscricaoId") // chave estrangeira pra inscricao
                );
        }
    }
}
