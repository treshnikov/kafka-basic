using System.Net;
using Microsoft.Extensions.Logging;
using Confluent.Kafka;

public class BookProducerService : IWorker
{
    private readonly ILogger<BookProducerService> _logger;
    private IProducer<Null, string> _producer;

    public BookProducerService(ILogger<BookProducerService> logger)
    {
        _logger = logger;
        var producerConfig = new ProducerConfig()
        {
            BootstrapServers = AppConfig.Host,
            ClientId = Dns.GetHostName()
        };
        _producer = new ProducerBuilder<Null, string>(producerConfig).Build();
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

                await _producer.ProduceAsync(AppConfig.Topic, new Message<Null, string>
                {
                    Value = System.Text.Json.JsonSerializer.Serialize(book)
                }, cancellationToken);

                _logger.LogInformation($"Produced: {book.Title}");

                i++;
                await Task.Delay(1, cancellationToken);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Producer stopped.");
            _producer?.Flush(cancellationToken);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _producer?.Dispose();
        return Task.CompletedTask;
    }
}
