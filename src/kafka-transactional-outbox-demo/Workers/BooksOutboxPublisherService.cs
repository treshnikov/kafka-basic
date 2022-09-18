using System.Net;
using Microsoft.Extensions.Logging;
using Confluent.Kafka;
using Microsoft.Extensions.Hosting;

public class BooksOutboxPublisherService : BackgroundService
{
    private readonly IBooksDbContext _dbContext;
    private readonly ILogger<BooksProducerService> _logger;
    private readonly IProducer<int, string> _producer;

    public BooksOutboxPublisherService(IBooksDbContext dbContext, ILogger<BooksProducerService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
        var producerConfig = new ProducerConfig()
        {
            BootstrapServers = AppConfig.Host,
            ClientId = Dns.GetHostName(),
            Acks = Acks.All
        };
        _producer = new ProducerBuilder<int, string>(producerConfig).Build();
        _logger.LogInformation("Publisher created.");
    }

    protected override async Task ExecuteAsync(CancellationToken stopToken)
    {
        try
        {
            int i = 1;
            while (!stopToken.IsCancellationRequested)
            {
                try
                {
                    var outbox = _dbContext.BooksOutbox.ToArray();
                    foreach (var book in outbox)
                    {
                        await SendBookToKafka(i, book.Data, stopToken);
                        _logger.LogInformation($"Book #{i} has been published from outbox");
                        _dbContext.BooksOutbox.Remove(book);
                    }
                    await _dbContext.SaveChangesAsync(stopToken);
                }
                catch (Exception e)
                {
                    _logger.LogError($"An error occurred while publishing books: {e.Message}");
                }

                await Task.Delay(TimeSpan.FromSeconds(3), stopToken);
                i++;
            }
        }
        catch (OperationCanceledException)
        {
            _producer?.Flush(stopToken);
            _producer.Dispose();

            _logger.LogWarning("Producer stopped.");
        }
    }

    private async Task SendBookToKafka(int i, string book, CancellationToken stopToken)
    {
        try
        {
            await _producer.ProduceAsync(new TopicPartition(AppConfig.Topic, new Partition(i % 50)), new Message<int, string>
            {
                Key = i,
                Value = book
            }, stopToken);

            //_logger.LogInformation($"Produced: {book.Title}");
        }
        catch (ProduceException<int, string> ex)
        {
            _logger.LogWarning($"A publisher exception has occured: {ex.Message}");
            throw;
        }
    }
}
