using Microsoft.EntityFrameworkCore;
using SistemaRH.Infra;
using SistemaRH.Infra.Interfaces;
using SistemaRH.Infra.Repositories;
using System;

namespace SubscriberProcesso
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // configuração do db context
            var optionsBuilder = new DbContextOptionsBuilder<SqlContext>();
            optionsBuilder.UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=SistemaRH;Trusted_Connection=True;");

            using var context = new SqlContext(optionsBuilder.Options);
            // repositorio de processos seletivos
            IProcessoRepository processoRepository = new ProcessoRepository(context);

            // subscriber de processos
            var subscriber = new ProcessoSubscriber(processoRepository);
            // chamada do método
            subscriber.EscutarFila();
        }
    }
}
