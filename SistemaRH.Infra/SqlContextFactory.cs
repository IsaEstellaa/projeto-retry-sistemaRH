using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaRH.Infra
{
    public class SqlContextFactory : IDesignTimeDbContextFactory<SqlContext>
    {
        public SqlContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<SqlContext>();

            // define a string de conexão para o SQL Server
            optionsBuilder.UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=SistemaRH;Trusted_Connection=True;");

            // retorna uma nova instância de SqlContext com as opções configuradas
            return new SqlContext(optionsBuilder.Options);
        }
    }
}
