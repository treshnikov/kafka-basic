using System.Net;
using Microsoft.Extensions.Logging;
using Confluent.Kafka;
using Microsoft.Extensions.Hosting;
using MediatR;

public class BooksOutboxPublisherService : INotificationHandler<NewMessageWasAddedIntoOutboxNotification>
{
    private readonly IBooksDbContext _dbContext;
    private readonly ILogger<BooksProducerService> _logger;

    public BooksOutboxPublisherService(IBooksDbContext dbContext, ILogger<BooksProducerService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task Handle(NewMessageWasAddedIntoOutboxNotification notification, CancellationToken stopToken)
    {
        try
        {
            var outbox = _dbContext.BooksOutbox.ToArray();
            using var producer = CreateProducer();
            foreach (var book in outbox)
            {
                await SendBookToKafka(producer, book.Data, stopToken);
                //_logger.LogInformation($"{book.Data} has been published from outbox");
                _dbContext.BooksOutbox.Remove(book);
            }
            await _dbContext.SaveChangesAsync(stopToken);
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Producer stopped.");
            throw;
        }
    }

    private static IProducer<int, string> CreateProducer()
    {
        var producerConfig = new ProducerConfig()
        {
            BootstrapServers = AppConfig.Host,
            ClientId = Dns.GetHostName(),
            Acks = Acks.All
        };

        return new ProducerBuilder<int, string>(producerConfig).Build();
    }

    private async Task SendBookToKafka(IProducer<int, string> producer, string book, CancellationToken stopToken)
    {
        try
        {
            var partition = new Partition(Math.Abs(book.GetHashCode() % AppConfig.TopicPartitionsNumber));
            await producer.ProduceAsync(new TopicPartition(AppConfig.Topic, partition), new Message<int, string>
            {
                Key = partition.Value,
                Value = book
            }, stopToken);
        }
        catch (ProduceException<int, string> ex)
        {
            _logger.LogWarning($"A publisher exception has occured: {ex.Message}");
            throw;
        }
    }
}
