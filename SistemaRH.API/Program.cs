using SistemaRH.Infra;
using Microsoft.EntityFrameworkCore;
using SistemaRH.Infra.Interfaces;
using SistemaRH.Infra.Repositories;
using SistemaRH.Infra.Messaging;

namespace SistemaRH.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Registrar o DbContext no container de dependências
            builder.Services.AddDbContext<SqlContext>(options =>
                options.UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=SistemaRH;Trusted_Connection=True;")
);

            builder.Services.AddScoped<IInscricaoRepository, InscricaoRepository>();
            builder.Services.AddScoped<IProcessoRepository, ProcessoRepository>();
            builder.Services.AddScoped<IVagaRepository, VagaRepository>();
            builder.Services.AddScoped<VagaPublisher>();
            builder.Services.AddScoped<ProcessoPublisher>();
            builder.Services.AddScoped<InscricaoPublisher>();

            builder.Services.AddControllers()
                            .AddJsonOptions(options =>
                            {
                                options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve;
                                options.JsonSerializerOptions.MaxDepth = 64; // ou o valor que desejar
                            });
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Configuração do Swagger
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseAuthorization();
            app.MapControllers();

            app.Run();
        }
    }
}
