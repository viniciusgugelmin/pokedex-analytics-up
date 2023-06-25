using System.Text;
using Confluent.Kafka;

namespace pokedex_analytics_up.Services;


public class KafkaService : BackgroundService
{
    private readonly ILogger<KafkaService> _logger;

    public KafkaService(ILogger<KafkaService> logger)
    {
        _logger = logger;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {

        var config = new ConsumerConfig
        {
            BootstrapServers = "localhost:9092", // Endereço do servidor Kafka
            GroupId = "meu-grupo-consumidor", // Identificador do grupo consumidor
            AutoOffsetReset = AutoOffsetReset.Earliest // Define o offset inicial para a leitura das mensagens
        };

        using var consumer = new ConsumerBuilder<Ignore, string>(config).Build();
        consumer.Subscribe("meu-topico"); // Nome do tópico que será consumido

        _logger.LogInformation("Kafka consumer started");

        stoppingToken.Register(() => consumer.Dispose());

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var consumeResult = consumer.Consume(stoppingToken);
                _logger.LogInformation($"Mensagem recebida do Kafka: {consumeResult.Message.Value}");
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Kafka consumer closed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao consumir mensagem do Kafka");
        }


        return Task.CompletedTask;
    }

    public async Task SendMessage(string message)
    {
        var config = new ProducerConfig { BootstrapServers = "localhost:9092" };

        // If serializers are not specified, default serializers from
        // `Confluent.Kafka.Serializers` will be automatically used where
        // available. Note: by default strings are encoded as UTF8.
        using (var p = new ProducerBuilder<Null, string>(config).Build())
        {
            try
            {
                var dr = await p.ProduceAsync("test-topic", new Message<Null, string> { Value="test" });
                Console.WriteLine($"Delivered '{dr.Value}' to '{dr.TopicPartitionOffset}'");
            }
            catch (ProduceException<Null, string> e)
            {
                Console.WriteLine($"Delivery failed: {e.Error.Reason}");
            }
        }
    }
}