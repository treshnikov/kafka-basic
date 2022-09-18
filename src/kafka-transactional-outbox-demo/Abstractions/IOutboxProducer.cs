public interface IOutboxProducer<T>
{
    Task ProduceAsync(T item, CancellationToken cancelationToken);
}