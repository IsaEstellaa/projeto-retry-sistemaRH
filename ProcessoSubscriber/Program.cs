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
            var optionsBuilder = new DbContextOptionsBuilder<SqlContext>();
            optionsBuilder.UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=SistemaRH;Trusted_Connection=True;");

            using var context = new SqlContext(optionsBuilder.Options);
            IProcessoRepository processoRepository = new ProcessoRepository(context);

            var subscriber = new ProcessoSubscriber(processoRepository);
            subscriber.EscutarFila();
        }
    }
}
