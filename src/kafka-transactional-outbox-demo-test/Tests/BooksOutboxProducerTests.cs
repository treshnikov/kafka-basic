namespace kafka_transactional_outbox_demo_test;
using Moq;
using Microsoft.Extensions.Logging;

public class BooksOutboxProducerTests : BaseTest
{

    [Test]
    public async Task Test1()
    {
        // arrange
        var loggerMock = new Mock<ILogger<BooksOutboxProducer>>();
        var logger = loggerMock.Object;
        var booksOutboxProducer = new BooksOutboxProducer(Context, logger);

        // act
        var book = new Book(
            Guid.NewGuid(),
            "Book #" + 1,
            "Author " + 1,
            DateTime.UtcNow);

        var cts = new CancellationTokenSource();
        await booksOutboxProducer.ProduceAsync(book, cts.Token);

        // assert
        Assert.AreEqual(Context.Books.ToArray().Length, 1);
    }
}