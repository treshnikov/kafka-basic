using Microsoft.Extensions.Logging;

public class BooksOutboxProducer : IOutboxProducer<Book>
{
    private readonly IBooksDbContext _dbContext;
    private readonly ILogger<BooksOutboxProducer> _logger;

    public BooksOutboxProducer(IBooksDbContext context, ILogger<BooksOutboxProducer> logger)
    {
        _dbContext = context;
        _logger = logger;
    }
    public async Task ProduceAsync(Book book, CancellationToken cancelationToken)
    {
        var transaction = await _dbContext.BeginTransactionAsync(cancelationToken);
        try
        {
            await _dbContext.Books.AddAsync(book, cancelationToken);

            var serializedBook = System.Text.Json.JsonSerializer.Serialize(book);
            await _dbContext.BooksOutbox.AddAsync(new BookOutbox { Data = serializedBook }, cancelationToken);
            await _dbContext.SaveChangesAsync(cancelationToken);
        }
        catch (Exception e)
        {
            await transaction.RollbackAsync(cancelationToken);
            _logger.LogError($"An error occurred while producing books: {e.Message}");
        }
        await transaction.CommitAsync(cancelationToken);
    }
}

