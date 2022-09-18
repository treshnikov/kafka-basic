using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using MediatR;

public class BooksProducerBackgroundService : BackgroundService
{
    private readonly ILogger<BooksProducerBackgroundService> _logger;
    private readonly IMediator _mediator;
    private readonly ITransactionalOutboxHandler<Book> _outboxBooksProducer;

    public BooksProducerBackgroundService(IMediator mediator, ITransactionalOutboxHandler<Book> outboxBooksProducer, ILogger<BooksProducerBackgroundService> logger)
    {
        _mediator = mediator;
        _outboxBooksProducer = outboxBooksProducer;
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

                await _outboxBooksProducer.HandleAsync(book, stopToken);

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