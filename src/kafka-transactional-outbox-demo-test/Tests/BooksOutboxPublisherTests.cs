namespace kafka_transactional_outbox_demo_test;

public class OutboxedBooksPublisherTests : BaseTest
{
    [Test]
    public async Task Should_Empty_Outbox_Table()
    {
        // arrange
        var loggerMock = new Mock<ILogger<OutboxedBooksPublisher>>();
        var kafkaBooksProducerMock = new Mock<IKafkaBooksProducer>();
        var booksOutboxPublisher = new OutboxedBooksPublisher(Context, kafkaBooksProducerMock.Object, loggerMock.Object);
        var cts = new CancellationTokenSource();

        Context.BooksOutbox.Add(new BookOutbox{Id = 1, Data = "123"});
        await Context.SaveChangesAsync(cts.Token);
        Assert.AreEqual(Context.BooksOutbox.ToArray().Length, 1);

        // act
        await booksOutboxPublisher.Handle(new NewMessageWasAddedIntoOutboxNotification(), cts.Token);

        // assert
        Assert.AreEqual(Context.BooksOutbox.ToArray().Length, 0);
    }
}