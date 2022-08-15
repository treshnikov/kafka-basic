using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Confluent.Kafka;

public class BookConsumerService : IHostedService
{
    private readonly ILogger<BookConsumerService> _logger;
    private IConsumer<Null, string> _consumer;

    public BookConsumerService(ILogger<BookConsumerService> logger)
    {
        _logger = logger;
        _logger.LogInformation("Consumer created.");
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var conf = new ConsumerConfig
        {
            GroupId = "group-1",
            BootstrapServers = AppConfig.Host,
            AutoOffsetReset = AutoOffsetReset.Earliest
        };

        using (var consumer = new ConsumerBuilder<Ignore, string>(conf).Build())
        {
            consumer.Subscribe(AppConfig.TopicName);
            try
            {
                while (true)
                {
                    try
                    {
                        var msg = consumer.Consume(cancellationToken);
                        var book = System.Text.Json.JsonSerializer.Deserialize<Book>(msg.Value);

                        _logger.LogInformation($"Consumed message '{System.Text.Json.JsonSerializer.Serialize(msg)}");
                    }
                    catch (ConsumeException e)
                    {
                        _logger.LogError($"Error occured: {e.Error.Reason}");
                    }
                }
            }
            catch (OperationCanceledException)
            {
                consumer.Close();
            }
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _consumer?.Dispose();
        return Task.CompletedTask;
    }
}
