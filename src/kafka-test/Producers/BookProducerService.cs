using System.Net;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Confluent.Kafka;

public class BookProducerService : IHostedService
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
        int i = 1;
        while (true)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                _logger.LogInformation("Producer stopped.");
            }

            var book = new Book
            {
                Id = Guid.NewGuid(),
                Title = "Book " + i,
                Author = "Author " + i,
                ReleaseDate = DateTime.Now
            };

            await _producer.ProduceAsync(AppConfig.TopicName, new Message<Null, string>
            {
                Value = System.Text.Json.JsonSerializer.Serialize(book)
            }, cancellationToken);

            _logger.LogInformation($"Message {i} has been sent");

            i++;
            await Task.Delay(1000, cancellationToken);
        }

        _producer.Flush(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _producer?.Dispose();
        return Task.CompletedTask;
    }
}
