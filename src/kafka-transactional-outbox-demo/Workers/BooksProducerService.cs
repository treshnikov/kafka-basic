using System.Net;
using Microsoft.Extensions.Logging;
using Confluent.Kafka;
using Microsoft.Extensions.Hosting;

public class BooksProducerService : BackgroundService
{
    private readonly IBooksDbContext _dbContext;
    private readonly ILogger<BooksProducerService> _logger;
    private IProducer<int, string> _producer;

    public BooksProducerService(IBooksDbContext dbContext, ILogger<BooksProducerService> logger)
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
                         DateTime.UtcNow);
                var transaction = await _dbContext.BeginTransactionAsync(stopToken);
                try
                {
                    await SaveBookToDb(book, stopToken);

                    var serializedBook = System.Text.Json.JsonSerializer.Serialize(book);
                    await SaveBookToOutbox(serializedBook, stopToken);

                    //todo - move this code to another service and gather books from the DB in order to implement the transactional outbox approach
                    await SendBookToKafka(i, serializedBook, stopToken);
                }
                catch (Exception e)
                {
                    await transaction.RollbackAsync(stopToken);
                    _logger.LogError($"An error occurred while producing books: {e.Message}");
                }
                await transaction.CommitAsync(stopToken);

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

    private async Task SaveBookToOutbox(string serializedBook, CancellationToken stopToken)
    {
        _dbContext.BooksOutbox.Add(new BookOutbox { Data = serializedBook });
        await _dbContext.SaveChangesAsync(stopToken);
    }

    private async Task SaveBookToDb(Book book, CancellationToken stopToken)
    {
        _dbContext.Books.Add(book);
        await _dbContext.SaveChangesAsync(stopToken);
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
            _logger.LogWarning($"A producer exception has occured: {ex.Message}");
            throw;
        }
    }
}
