using System.Text;
using Confluent.Kafka;

namespace pokedex_analytics_up.Services;


public class KafkaService : BackgroundService
{
    private readonly ILogger<KafkaService> _logger;
    private readonly IConfiguration _configuration;

    public KafkaService(ILogger<KafkaService> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var config = new ConsumerConfig
        {
            BootstrapServers = _configuration["Kafka:BootstrapServers"],
            GroupId = _configuration["Kafka:GroupId"],
            AutoOffsetReset = AutoOffsetReset.Earliest
        };

        using var consumer = new ConsumerBuilder<Ignore, string>(config).Build();
        consumer.Subscribe(_configuration["Kafka:Topic"]);

        _logger.LogInformation("Kafka consumer started");

        stoppingToken.Register(() => consumer.Dispose());

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var consumeResult = consumer.Consume(stoppingToken);
                _logger.LogInformation($"Mensagem recebida: {consumeResult.Message.Value}");
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Kafka consumer closed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao consumir mensagem do Kafka");
            }
        }

        return Task.CompletedTask;
    }

    public void SendMessage(string message)
    {
        var config = new ProducerConfig
        {
            BootstrapServers = _configuration["Kafka:BootstrapServers"]
        };

        using var producer = new ProducerBuilder<Null, string>(config).Build();

        try
        {
            var result = producer.ProduceAsync(_configuration["Kafka:Topic"], new Message<Null, string>
            {
                Value = message
            }).GetAwaiter().GetResult();

            _logger.LogInformation($"Mensagem enviada: {message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao enviar mensagem para o Kafka");
        }
    }
}