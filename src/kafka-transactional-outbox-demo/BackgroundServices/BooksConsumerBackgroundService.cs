using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Confluent.Kafka;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

public class BooksConsumerBackgroundService : BackgroundService
{
    private readonly ILogger<BooksConsumerBackgroundService> _logger;
    private readonly KafkaConfig _kafkaConfig;
    private readonly ConsumerConfig _consumerConfig;

    public BooksConsumerBackgroundService(ILogger<BooksConsumerBackgroundService> logger, IOptions<KafkaConfig> kafkaConfig)
    {
        _logger = logger;
        _kafkaConfig = kafkaConfig.Value;
        _consumerConfig = new()
        {
            GroupId = _kafkaConfig.ConsumerGroupName,
            BootstrapServers = _kafkaConfig.Host,
            AutoOffsetReset = AutoOffsetReset.Earliest,
        }; 
        _logger.LogInformation("Consumer created.");
    }

    protected override async Task ExecuteAsync(CancellationToken stopToken)
    {
        using var consumer = new ConsumerBuilder<Ignore, string>(_consumerConfig).Build();
        consumer.Subscribe(_kafkaConfig.Topic);
        try
        {
            while (!stopToken.IsCancellationRequested)
            {
                try
                {
                    var msg = consumer.Consume(stopToken);
                    var book = System.Text.Json.JsonSerializer.Deserialize<Book>(msg.Value);

                    _logger.LogInformation($"{book.Title} has been consumed from a partition #{msg.Partition.Value}");
                }
                catch (ConsumeException e)
                {
                    _logger.LogError($"Error occured: {e.Error.Reason}");
                }
            }
        }
        catch (OperationCanceledException)
        {
            consumer?.Close();
            consumer?.Dispose();

            _logger.LogWarning($"Consumer stopped.");
        }
    }
}
