using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

internal class BookOutboxConfiguration : IEntityTypeConfiguration<BookOutbox>
{
    public void Configure(EntityTypeBuilder<BookOutbox> builder)
    {
        builder.ToTable("BooksOutbox");
        builder.HasIndex(i => i.Id).IsUnique();
    }
}