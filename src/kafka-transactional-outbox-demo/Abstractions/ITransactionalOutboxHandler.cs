public interface ITransactionalOutboxHandler<TEntity, TOutboxEntity> where TEntity: class where TOutboxEntity: class  
{
    /// <summary>
    /// Saves an entity and an outboxed representation of the entity into the DB main and the outbox tables 
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="cancelationToken"></param>
    /// <returns></returns>
    Task HandleAsync(TEntity entity, TOutboxEntity outboxEntity, CancellationToken cancelationToken);
}