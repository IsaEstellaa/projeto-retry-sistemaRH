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
    public class InscricaoPublisher
    {
        private const string ExchangeName = "exchange-sistema-rh";
        private const string RoutingKey = "inscricao_cadastro";

        public void Publicar(Inscricao inscricao)
        {
            var factory = new ConnectionFactory()
            {
                Uri = new Uri("amqps://mfqmnsao:Q_x4fzXYXlr_maYttfoKKVtReE9tFGVl@jackal.rmq.cloudamqp.com/mfqmnsao")
            };

            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();

            // Declara a exchange
            channel.ExchangeDeclare(exchange: ExchangeName, type: ExchangeType.Direct, durable: true);

            var mensagem = JsonSerializer.Serialize(inscricao);
            var body = Encoding.UTF8.GetBytes(mensagem);

            channel.BasicPublish(
                exchange: ExchangeName,
                routingKey: RoutingKey,
                basicProperties: null,
                body: body
            );

            Console.WriteLine($"[InscricaoPublisher] Inscricao de candidato publicada: {mensagem}");
        }
    }
}
