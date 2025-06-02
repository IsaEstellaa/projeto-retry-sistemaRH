using RabbitMQ.Client;
using System;
using System.Text;
using System.Text.Json;

namespace SistemaRH.Publisher
{
    public class Publisher
    {
        private readonly string _uri = "amqps://mfqmnsao:Q_x4fzXYXlr_maYttfoKKVtReE9tFGVl@jackal.rmq.cloudamqp.com/mfqmnsao";
        private readonly string _exchangeName = "exchange-trabalho-fanout";

        // Método para enviar uma vaga para o RabbitMQ
        public void EnviarMensagemRabbitMQ(Vaga vaga)
        {
            var factory = new ConnectionFactory { Uri = new Uri(_uri) };

            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();

            // Declara a exchange
            channel.ExchangeDeclare(_exchangeName, "fanout");

            // Serializa o objeto Vaga para JSON
            var message = JsonSerializer.Serialize(vaga);
            var body = Encoding.UTF8.GetBytes(message);

            // Publica a mensagem
            channel.BasicPublish(exchange: _exchangeName,
                                 routingKey: "",
                                 basicProperties: null,
                                 body: body);

            Console.WriteLine($"Mensagem da vaga enviada para o RabbitMQ: {vaga.Titulo}");
        }

        // Modelos de dados
        public class Vaga
        {
            public string Titulo { get; set; }
            public string Descricao { get; set; }
            public string Localizacao { get; set; }
            public DateTime DataPublicacao { get; set; }
            public double? Salario { get; set; }
        }
    }
}
