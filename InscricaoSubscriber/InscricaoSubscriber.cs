﻿using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using SistemaRH.Domain;
using System;
using System.Text;
using System.Text.Json;
using SistemaRH.Infra.Repositories;
using SistemaRH.Infra.Interfaces;

namespace SubscriberInscricao
{
    public class InscricaoSubscriber
    {
        private readonly IInscricaoRepository _inscricaoRepository;

        // usada para roteamento das mensagens
        private const string ExchangeName = "exchange-sistema-rh";
        // fila principal
        private const string QueueName = "inscricao_cadastro";
        // fila de retyr
        private const string RetryQueueName = "inscricao_cadastro_retry";
        // fila quando passa do num de tentativas
        private const string DlqQueueName = "inscricao_cadastro_dlq";
        // chave de vinculação para exchange
        private const string RoutingKey = "inscricao_cadastro";

        private const int maxTentativas = 5;

        // injetando o repositório
        public InscricaoSubscriber(IInscricaoRepository inscricaoRepository)
        {
            _inscricaoRepository = inscricaoRepository;
        }

        // função que é chamada no program
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

            // fila de DLQ
            channel.QueueDeclare(queue: DlqQueueName, durable: true, exclusive: false, autoDelete: false, arguments: null);
            channel.QueueBind(queue: DlqQueueName, exchange: ExchangeName, routingKey: DlqQueueName);

            // declara a fila principal com DLQ configurada para enviar para a de retry
            var mainQueueArgs = new Dictionary<string, object>
            {
                { "x-dead-letter-exchange", ExchangeName },
                { "x-dead-letter-routing-key", RetryQueueName }
            };
            channel.QueueDeclare(queue: QueueName, durable: true, exclusive: false, autoDelete: false, arguments: mainQueueArgs);
            channel.QueueBind(queue: QueueName, exchange: ExchangeName, routingKey: RoutingKey);

            // fila de retry com TTL e DLQ de volta pra principal
            var retryQueueArgs = new Dictionary<string, object>
            {
                { "x-message-ttl", 5000 },
                { "x-dead-letter-exchange", ExchangeName },
                { "x-dead-letter-routing-key", RoutingKey }
            };
            channel.QueueDeclare(queue: RetryQueueName, durable: true, exclusive: false, autoDelete: false, arguments: retryQueueArgs);
            channel.QueueBind(queue: RetryQueueName, exchange: ExchangeName, routingKey: RetryQueueName);

            Console.WriteLine("[InscricaoSubscriber] Aguardando mensagens...");

            // consumidor das mensagens
            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var mensagem = Encoding.UTF8.GetString(body);
                Console.WriteLine($"[InscricaoSubscriber] Mensagem recebida: {mensagem}");

                // contador de tentativas
                int retryCount = 0;
                if (ea.BasicProperties.Headers != null && ea.BasicProperties.Headers.TryGetValue("x-retry-count", out var retryHeader))
                {
                    if (retryHeader is byte[] retryBytes)
                    {
                        var retryStr = Encoding.UTF8.GetString(retryBytes);
                        int.TryParse(retryStr, out retryCount);
                    }
                    else if (retryHeader is int retryInt)
                    {
                        retryCount = retryInt;
                    }
                    else if (retryHeader is long retryLong)
                    {
                        retryCount = (int)retryLong;
                    }
                }

                try
                {
                    var inscricao = JsonSerializer.Deserialize<Inscricao>(mensagem);
                    if (inscricao == null)
                    {
                        Console.WriteLine("[InscricaoSubscriber] Mensagem inválida.");
                        channel.BasicAck(ea.DeliveryTag, false);
                        return;
                    }

                    // aguarda pelo registro da inscrição no repositório
                    await _inscricaoRepository.RegistrarInscricao(inscricao);
                    Console.WriteLine($"[InscricaoSubscriber] A inscrição de '{inscricao.NomeCandidato}' foi registrada com sucesso!");
                    channel.BasicAck(ea.DeliveryTag, false);
                }
                catch (Exception ex)
                {
                    retryCount++;
                    Console.WriteLine($"[InscricaoSubscriber] Erro ao registrar inscrição: {ex.Message} :( Tentativa: {retryCount}");

                    if (retryCount >= maxTentativas)
                    {
                        Console.WriteLine("[InscricaoSubscriber] O máximo de tentativas foi atingido. A mensagem será descartada.");

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
                    Console.WriteLine($"[InscricaoSubscriber] Reenviando mensagem para fila de retry (tentativa {retryCount})...");
                }
            };

            // consome a fila com controle manual de confirmação
            channel.BasicConsume(queue: QueueName, autoAck: false, consumer: consumer);
            Console.WriteLine("Aguardando mensagens na fila inscricao_cadastro. Pressione ENTER para sair.");
            Console.ReadLine();
        }
    }
}
