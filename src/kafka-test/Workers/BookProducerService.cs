using System.Net;
using Microsoft.Extensions.Logging;
using Confluent.Kafka;
using Microsoft.Extensions.Hosting;

public class BookProducerService : BackgroundService
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

    protected override async Task ExecuteAsync(CancellationToken stopToken)
    {
        try
        {
            int i = 1;
            while (!stopToken.IsCancellationRequested)
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
                    }, stopToken);

                    //_logger.LogInformation($"Produced: {book.Title}");

                    i++;

                }
                catch (ProduceException<int, string> ex)
                {
                    _logger.LogWarning($"A producer exception has occured: {ex.Message}");

                }
                await Task.Delay(TimeSpan.FromSeconds(1), stopToken);
            }
        }
        catch (OperationCanceledException)
        {
            _producer?.Flush(stopToken);
            _producer.Dispose();

            _logger.LogWarning("Producer stopped.");
        }
    }
}
