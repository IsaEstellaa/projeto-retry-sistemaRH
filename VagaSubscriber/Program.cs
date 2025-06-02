using Microsoft.EntityFrameworkCore;
using SistemaRH.Infra;
using SistemaRH.Infra.Interfaces;
using SistemaRH.Infra.Repositories;
using System;

namespace SubscriberVaga
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // configuração do db context
            var optionsBuilder = new DbContextOptionsBuilder<SqlContext>();
            optionsBuilder.UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=SistemaRH;Trusted_Connection=True;");

            using var context = new SqlContext(optionsBuilder.Options);
            // repositorio de vaga
            IVagaRepository vagaRepository = new VagaRepository(context);

            // subscriber de vaga
            var subscriber = new VagaSubscriber(vagaRepository);
            // chamada do método
            subscriber.EscutarFila();
        }
    }
}
