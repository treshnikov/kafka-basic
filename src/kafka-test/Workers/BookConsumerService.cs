using Microsoft.Extensions.Logging;
using Confluent.Kafka;

public class BookConsumerService : IWorker
{
    private readonly ILogger<BookConsumerService> _logger;

    public BookConsumerService(ILogger<BookConsumerService> logger)
    {
        _logger = logger;
        _logger.LogInformation("Consumer created.");
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var conf = new ConsumerConfig
        {
            GroupId = "book-consumers",
            BootstrapServers = AppConfig.Host,
            AutoOffsetReset = AutoOffsetReset.Earliest
        };

        using var consumer = new ConsumerBuilder<Ignore, string>(conf).Build();
        consumer.Subscribe(AppConfig.Topic);
        try
        {
            while (true)
            {
                try
                {
                    var msg = consumer.Consume(cancellationToken);
                    var book = System.Text.Json.JsonSerializer.Deserialize<Book>(msg.Value);

                    _logger.LogInformation($"Consumed: '{book.Title}");
                }
                catch (ConsumeException e)
                {
                    _logger.LogError($"Error occured: {e.Error.Reason}");
                }
            }
        }
        catch (OperationCanceledException)
        {
            consumer?.Dispose();
            consumer?.Close();
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
