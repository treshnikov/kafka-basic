using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using MediatR;

public class BooksProducerService : BackgroundService
{
    private readonly IBooksDbContext _dbContext;
    private readonly ILogger<BooksProducerService> _logger;
    private readonly IMediator _mediator;

    public BooksProducerService(IBooksDbContext dbContext, IMediator mediator, ILogger<BooksProducerService> logger)
    {
        this._mediator = mediator;
        _dbContext = dbContext;
        _logger = logger;
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
                        "Book #" + i,
                        "Author " + i,
                         DateTime.UtcNow);

                var transaction = await _dbContext.BeginTransactionAsync(stopToken);
                try
                {
                    await _dbContext.Books.AddAsync(book, stopToken);

                    var serializedBook = System.Text.Json.JsonSerializer.Serialize(book);
                    await _dbContext.BooksOutbox.AddAsync(new BookOutbox { Data = serializedBook }, stopToken);
                    await _dbContext.SaveChangesAsync(stopToken);
                }
                catch (Exception e)
                {
                    await transaction.RollbackAsync(stopToken);
                    _logger.LogError($"An error occurred while producing books: {e.Message}");
                }
                await transaction.CommitAsync(stopToken);

                // send notification to the outbox
                await _mediator.Publish(new NewMessageWasAddedIntoOutboxNotification(), stopToken);
                _logger.LogInformation($"Book #{i} has been saved into the DB and added to the Outbox.");

                await Task.Delay(TimeSpan.FromSeconds(1), stopToken);
                i++;
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Producer stopped.");
        }
    }
}