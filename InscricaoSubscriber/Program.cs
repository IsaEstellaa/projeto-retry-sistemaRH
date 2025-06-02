using Microsoft.EntityFrameworkCore;
using SistemaRH.Infra;
using SistemaRH.Infra.Interfaces;
using SistemaRH.Infra.Repositories;
using System;

namespace SubscriberInscricao
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // configuração do db context
            var optionsBuilder = new DbContextOptionsBuilder<SqlContext>();
            optionsBuilder.UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=SistemaRH;Trusted_Connection=True;");

            using var context = new SqlContext(optionsBuilder.Options);
            // repositorio de inscricao
            IInscricaoRepository inscricaoRepository = new InscricaoRepository(context);

            // subscriber de inscricao
            var subscriber = new InscricaoSubscriber(inscricaoRepository);
            // chamada do método
            subscriber.EscutarFila();
        }
    }
}
