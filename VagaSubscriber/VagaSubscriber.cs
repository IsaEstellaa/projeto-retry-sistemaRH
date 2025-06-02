using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using SistemaRH.Domain;
using System;
using System.Text;
using System.Text.Json;
using SistemaRH.Infra.Repositories;
using SistemaRH.Infra.Interfaces;

namespace SubscriberVaga
{
    public class VagaSubscriber
    {
        private readonly IVagaRepository _vagaRepository;

        private const string ExchangeName = "exchange-sistema-rh"; // exchange
        private const string QueueName = "vaga_cadastro"; // fila principal (usada para o cadastro)
        private const string RetryQueueName = "vaga_cadastro_retry"; // fila de retryi
        private const string DlqQueueName = "vaga_cadastro_dlq"; // fila dead letter para mensagens descartadas
        private const string RoutingKey = "vaga_cadastro"; // routing key

        // maximo de tentativas
        private const int maxTentativas = 5;

        // injetando o repositório
        public VagaSubscriber(IVagaRepository vagaRepository)
        {
            _vagaRepository = vagaRepository;
        }

        public void EscutarFila()
        {
            var factory = new ConnectionFactory()
            {
                Uri = new Uri("amqps://mfqmnsao:Q_x4fzXYXlr_maYttfoKKVtReE9tFGVl@jackal.rmq.cloudamqp.com/mfqmnsao")
            };

            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();

            // exchange do tipo direct
            channel.ExchangeDeclare(exchange: ExchangeName, type: ExchangeType.Direct, durable: true);

            // declarando a fila DLQ
            channel.QueueDeclare(queue: DlqQueueName, durable: true, exclusive: false, autoDelete: false, arguments: null);
            channel.QueueBind(queue: DlqQueueName, exchange: ExchangeName, routingKey: DlqQueueName);

            // declaranod a principal, com dead letter para a fila de retry
            var queueArgs = new System.Collections.Generic.Dictionary<string, object>
            {
                { "x-dead-letter-exchange", ExchangeName },
                { "x-dead-letter-routing-key", RetryQueueName }
            };
            channel.QueueDeclare(queue: QueueName, durable: true, exclusive: false, autoDelete: false, arguments: queueArgs);
            channel.QueueBind(queue: QueueName, exchange: ExchangeName, routingKey: RoutingKey);

            // declarando a fila de retry, com TTL e dead letter para a fila principal
            var retryQueueArgs = new System.Collections.Generic.Dictionary<string, object>
            {
                { "x-message-ttl", 5000 },                     // espera 5 segundos
                { "x-dead-letter-exchange", ExchangeName },
                { "x-dead-letter-routing-key", RoutingKey }
            };
            channel.QueueDeclare(queue: RetryQueueName, durable: true, exclusive: false, autoDelete: false, arguments: retryQueueArgs);
            channel.QueueBind(queue: RetryQueueName, exchange: ExchangeName, routingKey: RetryQueueName);

            Console.WriteLine("[VagaSubscriber] Aguardando mensagens...");

            // consumidor das mensagens
            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var mensagem = Encoding.UTF8.GetString(body);

                Console.WriteLine($"[VagaSubscriber] Mensagem recebida: {mensagem}");

                // contador de tentativas
                int retryCount = 0;
                // le o cabeçalho
                if (ea.BasicProperties.Headers != null && ea.BasicProperties.Headers.TryGetValue("x-retry-count", out var retryHeader))
                {
                    if (retryHeader is byte[] retryBytes) // converte pra string e depois inteiro
                    {
                        var retryStr = Encoding.UTF8.GetString(retryBytes);
                        int.TryParse(retryStr, out retryCount);
                    }
                    else if (retryHeader is int retryInt) // inteiro
                    {
                        retryCount = retryInt;
                    }
                    else if (retryHeader is long retryLong) // long pra int
                    {
                        retryCount = (int)retryLong;
                    }
                }


                try
                {
                    var vaga = JsonSerializer.Deserialize<Vaga>(mensagem);
                    if (vaga == null)
                    {
                        Console.WriteLine("[VagaSubscriber] Mensagem inválida.");
                        // ACK - acknowledgment, foi processada, mas n vou enviar pra fila nenhuma
                        channel.BasicAck(ea.DeliveryTag, false); // remove da fila
                        return;
                    }

                    // aguarda pelo registro da vaga no repositório
                    await _vagaRepository.RegistrarVaga(vaga);
                    Console.WriteLine($"[VagaSubscriber] Vaga '{vaga.Titulo}' registrada com sucesso.");
                    channel.BasicAck(ea.DeliveryTag, false); // deu bom, confirma a mensagem
                }
                catch (Exception ex)
                {
                    retryCount++;
                    Console.WriteLine($"[VagaSubscriber] Erro ao registrar vaga: {ex.Message} :( Tentativa: {retryCount}.");

                    if (retryCount >= maxTentativas)
                    {
                        Console.WriteLine("[VagaSubscriber] O máximo de tentativas foi atingido. Sua mensagem será descartada, sinto muito :(");

                        var propsDlq = channel.CreateBasicProperties();
                        propsDlq.Persistent = true;
                        propsDlq.Headers = ea.BasicProperties.Headers ?? new Dictionary<string, object>();
                        propsDlq.Headers["x-retry-count"] = retryCount;

                        // publica diretamente na fila DLQ
                        channel.BasicPublish(
                            exchange: ExchangeName,
                            routingKey: DlqQueueName,
                            basicProperties: propsDlq,
                            body: body);

                        // ACK para remover da fila atual, para não ficar várias
                        channel.BasicAck(ea.DeliveryTag, false);
                        return;
                    }

                    // reenvia a mensagem para a fila de retry atualizando o contador no header
                    var props = channel.CreateBasicProperties();
                    props.Persistent = true;
                    props.Headers ??= new Dictionary<string, object>();
                    props.Headers["x-retry-count"] = retryCount;

                    // publica na minha fila de retry
                    channel.BasicPublish(
                        exchange: ExchangeName,
                        routingKey: RetryQueueName,
                        basicProperties: props,
                        body: body
                    );

                    // sem requeue da mensagem original (já está reenviando manualmente)
                    channel.BasicAck(ea.DeliveryTag, false);

                    Console.WriteLine($"[VagaSubscriber] Tentando novamente... :o (tentativa {retryCount}).");
                }
            };

            // consome a fila com controle manual de confirmação
            channel.BasicConsume(queue: QueueName, autoAck: false, consumer: consumer);
            Console.WriteLine("Aguardando mensagens na fila vaga_cadastro. Pressione ENTER para sair.");
            Console.ReadLine();
        }
    }
}
