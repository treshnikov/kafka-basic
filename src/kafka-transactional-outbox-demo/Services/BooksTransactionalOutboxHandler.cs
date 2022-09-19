using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

public class BooksTransactionalOutboxHandler : TransactionalOutboxHandlerBase<Book, BookOutbox>
{
    public BooksTransactionalOutboxHandler(IBooksDbContext context, ILogger<BooksTransactionalOutboxHandler> logger)
     : base(context as DbContext, logger)
    {
    }
}
