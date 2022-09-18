using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

public static class AppTestContext
{
    public static BooksDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<BooksDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        var context = new BooksDbContext(options);
        context.Database.EnsureCreated();

        DbInitializer.PopulateDefaultRecords(context);
        return context;
    }

    public static void DestroyDbContext(BooksDbContext context)
    {
        context.Database.EnsureDeleted();
        context.Dispose();
    }
}