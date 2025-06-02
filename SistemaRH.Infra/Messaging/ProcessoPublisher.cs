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
    public class ProcessoPublisher
    {
        // exchange onde as mensagens serão publicadas
        private const string ExchangeName = "exchange-sistema-rh";
        // routing key usada para direcionar a mensagem para a fila correta
        private const string RoutingKey = "processo_cadastro";

        // publica a mensagem
        public void Publicar(ProcessoSeletivo processo)
        {
            var factory = new ConnectionFactory()
            {
                Uri = new Uri("amqps://mfqmnsao:Q_x4fzXYXlr_maYttfoKKVtReE9tFGVl@jackal.rmq.cloudamqp.com/mfqmnsao")
            };

            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();

            // declara a exchange
            channel.ExchangeDeclare(exchange: ExchangeName, type: ExchangeType.Direct, durable: true);

            // serializa para json e converte para array
            var mensagem = JsonSerializer.Serialize(processo);
            var body = Encoding.UTF8.GetBytes(mensagem);

            // publica na exchange com o caminho correto
            channel.BasicPublish(
                exchange: ExchangeName,
                routingKey: RoutingKey,
                basicProperties: null,
                body: body
            );

            Console.WriteLine($"[ProcessoPublisher] Processo seletivo publicado: {mensagem}");
        }
    }
}
