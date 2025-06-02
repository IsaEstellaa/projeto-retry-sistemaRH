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

            // registrar o DbContext no container de dependencias
            builder.Services.AddDbContext<SqlContext>(options =>
                options.UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=SistemaRH;Trusted_Connection=True;")
);

            // registra os repositórios para serem injetados nas classes que os consomem
            builder.Services.AddScoped<IInscricaoRepository, InscricaoRepository>();
            builder.Services.AddScoped<IProcessoRepository, ProcessoRepository>();
            builder.Services.AddScoped<IVagaRepository, VagaRepository>();
            // registro dos publishers
            builder.Services.AddScoped<VagaPublisher>();
            builder.Services.AddScoped<ProcessoPublisher>();
            builder.Services.AddScoped<InscricaoPublisher>();

            // controllers de api e opcoes de serialização
            builder.Services.AddControllers()
                            .AddJsonOptions(options =>
                            {
                                options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve;
                                options.JsonSerializerOptions.MaxDepth = 64;
                            });
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // configuracao do Swagger
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
