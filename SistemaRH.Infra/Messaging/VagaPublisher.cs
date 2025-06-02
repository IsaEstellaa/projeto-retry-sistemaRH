using RabbitMQ.Client;
using SistemaRH.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SistemaRH.Infra.Messaging
{
    public class VagaPublisher
    {
        private const string ExchangeName = "exchange-sistema-rh";
        private const string RoutingKey = "vaga_cadastro";

        public void Publicar(Vaga vaga)
        {
            var factory = new ConnectionFactory()
            {
                Uri = new Uri("amqps://mfqmnsao:Q_x4fzXYXlr_maYttfoKKVtReE9tFGVl@jackal.rmq.cloudamqp.com/mfqmnsao")
            };

            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();

            // Declara a exchange
            channel.ExchangeDeclare(exchange: ExchangeName, type: ExchangeType.Direct, durable: true);

            var mensagem = JsonSerializer.Serialize(vaga);
            var body = Encoding.UTF8.GetBytes(mensagem);

            channel.BasicPublish(
                exchange: ExchangeName,
                routingKey: RoutingKey,
                basicProperties: null,
                body: body
            );

            Console.WriteLine($"[VagaPublisher] Vaga publicada: {mensagem}");
        }
    }
}
