public interface ITransactionalOutboxHandler<T>
{
    /// <summary>
    /// Saves and item both into the DB main and the outbox table 
    /// </summary>
    /// <param name="item"></param>
    /// <param name="cancelationToken"></param>
    /// <returns></returns>
    Task HandleAsync(T item, CancellationToken cancelationToken);
}