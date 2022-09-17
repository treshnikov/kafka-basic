using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Confluent.Kafka;
using Microsoft.Extensions.Hosting;

public class BookConsumerService : BackgroundService
{
    private readonly ILogger<BookConsumerService> _logger;

    public BookConsumerService(ILogger<BookConsumerService> logger)
    {
        _logger = logger;
        _logger.LogInformation("Consumer created.");
    }

    protected override async Task ExecuteAsync(CancellationToken stopToken)
    {

        var conf = new ConsumerConfig
        {
            GroupId = AppConfig.ConsumerGroupName,
            BootstrapServers = AppConfig.Host,
            AutoOffsetReset = AutoOffsetReset.Earliest,
        };

        using var consumer = new ConsumerBuilder<Ignore, string>(conf).Build();
        consumer.Subscribe(AppConfig.Topic);

        // for test - consume messages only from the fifth partition
        //consumer.Assign(new TopicPartition(AppConfig.Topic, new Partition(5){}));

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
            _logger.LogError($"Consumer stopped.");
        }
        finally
        {
            consumer?.Close();
            consumer?.Dispose();
        }
    }
}
