using System.Net;
using Microsoft.Extensions.Logging;
using Confluent.Kafka;
using Microsoft.Extensions.Hosting;

public class BookProducerService : BackgroundService
{
    private readonly IBooksDbContext _dbContext;
    private readonly ILogger<BookProducerService> _logger;
    private IProducer<int, string> _producer;

    public BookProducerService(IBooksDbContext dbContext, ILogger<BookProducerService> logger)
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

                await _dbContext.BeginTransactionAsync(stopToken);
                try
                {
                    await SaveBookToDb(book, stopToken);
                    await SendBookToKafka(i, book, stopToken);
                }
                catch(Exception e)
                {
                    await _dbContext.RollbackTransactionAsync(stopToken);
                    _logger.LogError(e.Message);
                }
                await _dbContext.CommitTransactionAsync(stopToken);

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

    private async Task SaveBookToDb(Book book, CancellationToken stopToken)
    {
        _dbContext.Books.Add(book);
        await _dbContext.SaveChangesAsync(stopToken);
    }

    private async Task SendBookToKafka(int i, Book book, CancellationToken stopToken)
    {
        try
        {
            await _producer.ProduceAsync(new TopicPartition(AppConfig.Topic, new Partition(i % 50)), new Message<int, string>
            {
                Key = i,
                Value = System.Text.Json.JsonSerializer.Serialize(book)
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
