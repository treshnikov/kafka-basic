using Microsoft.EntityFrameworkCore;

public interface IBooksDbContext
{
    DbSet<Book> Books { get; set; }
    DbSet<BookOutbox> BooksOutbox { get; set; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
