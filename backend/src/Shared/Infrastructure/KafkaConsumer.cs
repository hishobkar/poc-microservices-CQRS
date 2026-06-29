using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace RealWorldApp.Shared.Infrastructure;

public abstract class KafkaConsumer<T> : BackgroundService
{
    private readonly IConsumer<string, string> _consumer;
    protected readonly IServiceProvider _serviceProvider;
    private readonly ILogger<KafkaConsumer<T>> _logger;
    private readonly string _topic;

    protected KafkaConsumer(IConfiguration configuration, IServiceProvider serviceProvider, 
        ILogger<KafkaConsumer<T>> logger, string topic, string groupId)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _topic = topic;

        var config = new ConsumerConfig
        {
            BootstrapServers = configuration["Kafka:BootstrapServers"] ?? "localhost:9092",
            GroupId = groupId,
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false,
            EnablePartitionEof = true,
            AllowAutoCreateTopics = true
        };

        _consumer = new ConsumerBuilder<string, string>(config).Build();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _consumer.Subscribe(_topic);

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var consumeResult = _consumer.Consume(stoppingToken);
                    
                    if (consumeResult?.Message == null || consumeResult.IsPartitionEOF)
                        continue;

                    _logger.LogInformation("Received message from topic {Topic}, Partition: {Partition}, Offset: {Offset}", 
                        _topic, consumeResult.Partition, consumeResult.Offset);

                    using var scope = _serviceProvider.CreateScope();
                    var message = JsonSerializer.Deserialize<T>(consumeResult.Message.Value);
                    
                    if (message != null)
                    {
                        await HandleMessageAsync(message, stoppingToken);
                        _consumer.Commit(consumeResult);
                    }
                }
                catch (ConsumeException ex)
                {
                    _logger.LogError(ex, "Error consuming message from topic {Topic}", _topic);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing message from topic {Topic}", _topic);
                }
            }
        }
        finally
        {
            _consumer.Close();
            _consumer.Dispose();
        }
    }

    protected abstract Task HandleMessageAsync(T message, CancellationToken cancellationToken);
}