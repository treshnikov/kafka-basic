namespace kafka_transactional_outbox_demo_test;

public class BooksTransactionalOutboxHandlerTests : BaseTest
{
    [Test]
    public async Task Should_Add_Records_Both_To_Books_And_Outbox_Tables()
    {
        // arrange
        var loggerMock = new Mock<ILogger<BooksTransactionalOutboxHandler>>();
        var booksOutboxProducer = new BooksTransactionalOutboxHandler(Context, loggerMock.Object);

        // act
        var book = new Book(
            Guid.NewGuid(),
            "Book #" + 1,
            "Author " + 1,
            DateTime.UtcNow);
        var serializedBook = System.Text.Json.JsonSerializer.Serialize(book);
        var outboxBook = new BookOutbox { Data = serializedBook };

        var cts = new CancellationTokenSource();
        await booksOutboxProducer.HandleAsync(book, outboxBook, cts.Token);

        // assert
        Assert.AreEqual(Context.Books.ToArray().Length, 1);
        Assert.AreEqual(Context.BooksOutbox.ToArray().Length, 1);
    }
}