using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

public class BooksDbContext : DbContext, IBooksDbContext
{
    public DbSet<Book> Books { get; set; }
    public DbSet<BookOutbox> BooksOutbox { get; set; }

    public BooksDbContext(DbContextOptions<BooksDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfiguration(new BookOutboxConfiguration());
        builder.ApplyConfiguration(new BookConfiguration());
        base.OnModelCreating(builder);
    }

    public async Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken)
    {
        return await Database.BeginTransactionAsync(cancellationToken);
    }
}