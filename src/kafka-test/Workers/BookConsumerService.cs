using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Confluent.Kafka;
using Microsoft.Extensions.Hosting;

public class BookConsumerService : BackgroundService
{
    private readonly ILogger<BookConsumerService> _logger;
    private readonly ConsumerConfig _consumerConfig = new()
    {
        GroupId = AppConfig.ConsumerGroupName,
        BootstrapServers = AppConfig.Host,
        AutoOffsetReset = AutoOffsetReset.Earliest,
    };

    public BookConsumerService(ILogger<BookConsumerService> logger)
    {
        _logger = logger;
        _logger.LogInformation("Consumer created.");
    }

    protected override async Task ExecuteAsync(CancellationToken stopToken)
    {
        using var consumer = new ConsumerBuilder<Ignore, string>(_consumerConfig).Build();
        consumer.Subscribe(AppConfig.Topic);
        try
        {

            while (!stopToken.IsCancellationRequested)
            {
                try
                {
                    var msg = consumer.Consume(stopToken);
                    var book = System.Text.Json.JsonSerializer.Deserialize<Book>(msg.Value);

                    _logger.LogInformation($"Consumed: '{book.Title} from a partition #{msg.Partition.Value}");
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
