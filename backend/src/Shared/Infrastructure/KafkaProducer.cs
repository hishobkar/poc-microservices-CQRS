using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace RealWorldApp.Shared.Infrastructure;

public interface IKafkaProducer
{
    Task PublishAsync<T>(string topic, T message, string? key = null);
}

public class KafkaProducer : IKafkaProducer, IDisposable
{
    private readonly IProducer<string, string> _producer;
    private readonly ILogger<KafkaProducer> _logger;

    public KafkaProducer(IConfiguration configuration, ILogger<KafkaProducer> logger)
    {
        _logger = logger;
        var config = new ProducerConfig
        {
            BootstrapServers = configuration["Kafka:BootstrapServers"] ?? "localhost:9092",
            ClientId = configuration["Kafka:ClientId"] ?? "realworld-app",
            Acks = Acks.All,
            EnableIdempotence = true,
            MessageSendMaxRetries = 3,
            RetryBackoffMs = 1000
        };

        _producer = new ProducerBuilder<string, string>(config).Build();
    }

    public async Task PublishAsync<T>(string topic, T message, string? key = null)
    {
        try
        {
            var json = JsonSerializer.Serialize(message);
            var messageKey = key ?? Guid.NewGuid().ToString();
            
            var result = await _producer.ProduceAsync(topic, new Message<string, string>
            {
                Key = messageKey,
                Value = json
            });

            _logger.LogInformation("Published message to topic {Topic}, Partition: {Partition}, Offset: {Offset}", 
                topic, result.Partition, result.Offset);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error publishing message to topic {Topic}", topic);
            throw;
        }
    }

    public void Dispose()
    {
        _producer?.Dispose();
    }
}