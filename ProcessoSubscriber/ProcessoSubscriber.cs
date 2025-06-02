using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using SistemaRH.Domain;
using System;
using System.Text;
using System.Text.Json;
using SistemaRH.Infra.Repositories;
using SistemaRH.Infra.Interfaces;

namespace SubscriberProcesso
{
    public class ProcessoSubscriber
    {
        private readonly IProcessoRepository _processoRepository;

        private const string ExchangeName = "exchange-sistema-rh"; // exchange
        private const string QueueName = "processo_cadastro"; // fila principal
        private const string RetryQueueName = "processo_cadastro_retry"; // fila de retry
        private const string DlqQueueName = "processo_cadastro_dlq"; // fila dead letter para mensagens descartadas
        private const string RoutingKey = "processo_cadastro"; // routing key

        // maximo de tentativas
        private const int maxTentativas = 5;

        // injetando o repositório
        public ProcessoSubscriber(IProcessoRepository processoRepository)
        {
            _processoRepository = processoRepository;
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

            // fila principal com DLQ para a de retry
            var queueArgs = new Dictionary<string, object>
            {
                { "x-dead-letter-exchange", ExchangeName },
                { "x-dead-letter-routing-key", RetryQueueName }
            };
            channel.QueueDeclare(queue: QueueName, durable: true, exclusive: false, autoDelete: false, arguments: queueArgs);
            channel.QueueBind(queue: QueueName, exchange: ExchangeName, routingKey: RoutingKey);

            // fila de retry com TTL e DLQ de volta para principal
            var retryArgs = new Dictionary<string, object>
            {
                { "x-message-ttl", 5000 }, // 5 segundos de espera
                { "x-dead-letter-exchange", ExchangeName },
                { "x-dead-letter-routing-key", RoutingKey }
            };
            channel.QueueDeclare(queue: RetryQueueName, durable: true, exclusive: false, autoDelete: false, arguments: retryArgs);
            channel.QueueBind(queue: RetryQueueName, exchange: ExchangeName, routingKey: RetryQueueName);

            Console.WriteLine("[ProcessoSubscriber] Aguardando mensagens...");

            // consumidor das mensagens
            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var mensagem = Encoding.UTF8.GetString(body);
                Console.WriteLine($"[ProcessoSubscriber] Mensagem recebida: {mensagem}");

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
                    var processo = JsonSerializer.Deserialize<ProcessoSeletivo>(mensagem);
                    if (processo == null)
                    {
                        Console.WriteLine("[ProcessoSubscriber] Mensagem inválida.");
                        channel.BasicAck(ea.DeliveryTag, false); // remove da fila
                        return;
                    }

                    // aguarda pelo registro do processo no repositório
                    await _processoRepository.RegistrarProcessoSeletivo(processo);
                    Console.WriteLine($"[ProcessoSubscriber] Processo seletivo '{processo.Nome}' registrado com sucesso.");
                    channel.BasicAck(ea.DeliveryTag, false);
                }
                catch (Exception ex)
                {
                    retryCount++;
                    Console.WriteLine($"[ProcessoSubscriber] Erro ao registrar processo: {ex.Message} :( Tentativa: {retryCount}.");

                    if (retryCount >= maxTentativas)
                    {
                        Console.WriteLine("[ProcessoSubscriber] O máximo de tentativas foi atingido. Sua mensagem será descartada, sinto muito :(");

                        // envia pra DLQ
                        var propsDlq = channel.CreateBasicProperties();
                        propsDlq.Persistent = true;
                        propsDlq.Headers = ea.BasicProperties.Headers ?? new Dictionary<string, object>();
                        propsDlq.Headers["x-retry-count"] = retryCount;

                        channel.BasicPublish(
                            exchange: ExchangeName,
                            routingKey: DlqQueueName,
                            basicProperties: propsDlq,
                            body: body);

                        channel.BasicAck(ea.DeliveryTag, false);
                        return;
                    }

                    // envia ou reenvia para a de retyr, mandando o contador
                    var props = channel.CreateBasicProperties();
                    props.Persistent = true;
                    props.Headers ??= new Dictionary<string, object>();
                    props.Headers["x-retry-count"] = retryCount;

                    channel.BasicPublish(
                        exchange: ExchangeName,
                        routingKey: RetryQueueName,
                        basicProperties: props,
                        body: body
                    );

                    channel.BasicAck(ea.DeliveryTag, false);
                    Console.WriteLine($"[ProcessoSubscriber] Tentando novamente... :o (tentativa {retryCount}).");
                }
            };

            // consome a fila com controle manual de confirmação
            channel.BasicConsume(queue: QueueName, autoAck: false, consumer: consumer);
            Console.WriteLine("Aguardando mensagens na fila processo_cadastro. Pressione ENTER para sair.");
            Console.ReadLine();
        }
    }
}
