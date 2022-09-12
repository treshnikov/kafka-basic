using System.Net;
using Microsoft.Extensions.Logging;
using Confluent.Kafka;

public class BookProducerService : IWorker
{
    private readonly ILogger<BookProducerService> _logger;
    private IProducer<int, string> _producer;

    public BookProducerService(ILogger<BookProducerService> logger)
    {
        _logger = logger;
        var producerConfig = new ProducerConfig()
        {
            BootstrapServers = AppConfig.Host,
            ClientId = Dns.GetHostName(),
            Acks = Acks.All
        };
        _producer = new ProducerBuilder<int, string>(producerConfig).Build();
        _logger.LogInformation("Producer created.");
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            int i = 1;
            while (true)
            {
                var book = new Book(
                        Guid.NewGuid(),
                        "Book " + i,
                        "Author " + i,
                         DateTime.Now);

                try
                {
                    await _producer.ProduceAsync(new TopicPartition(AppConfig.Topic, new Partition(i % 50)), new Message<int, string>
                    {
                        Key = i,
                        Value = System.Text.Json.JsonSerializer.Serialize(book)
                    }, cancellationToken);

                    //_logger.LogInformation($"Produced: {book.Title}");

                    i++;

                }
                catch (ProduceException<int, string> ex)
                {

                    _logger.LogError($"A producer exception has occured: {ex.Message}");

                }
                await Task.Delay(500, cancellationToken);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Producer stopped.");
            _producer?.Flush(cancellationToken);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _producer?.Dispose();
        return Task.CompletedTask;
    }
}
