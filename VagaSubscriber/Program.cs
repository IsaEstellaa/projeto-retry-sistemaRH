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
            var optionsBuilder = new DbContextOptionsBuilder<SqlContext>();
            optionsBuilder.UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=SistemaRH;Trusted_Connection=True;");

            using var context = new SqlContext(optionsBuilder.Options);
            IVagaRepository vagaRepository = new VagaRepository(context);

            var subscriber = new VagaSubscriber(vagaRepository);
            subscriber.EscutarFila();
        }
    }
}
