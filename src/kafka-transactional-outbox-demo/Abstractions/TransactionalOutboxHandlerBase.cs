using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

public abstract class TransactionalOutboxHandlerBase<TEntity, TOutboxEntity> : ITransactionalOutboxHandler<TEntity, TOutboxEntity>  where TEntity: class where TOutboxEntity: class
{
    private readonly DbContext _dbContext;
    private readonly ILogger Logger;

    public TransactionalOutboxHandlerBase(DbContext context, ILogger logger)
    {
        _dbContext = context;
        Logger = logger;
    }
    public async Task HandleAsync(TEntity entity, TOutboxEntity outboxEntity, CancellationToken cancelationToken)
    {
        var transaction = await _dbContext.Database.BeginTransactionAsync(cancelationToken);
        try
        {
            //(_dbContext as DbContext).Set<TEnt>
            await _dbContext.Set<TEntity>().AddAsync(entity, cancelationToken);
            await _dbContext.Set<TOutboxEntity>().AddAsync(outboxEntity, cancelationToken);
            await _dbContext.SaveChangesAsync(cancelationToken);
        }
        catch (Exception e)
        {
            await transaction.RollbackAsync(cancelationToken);
            Logger.LogError($"An error occurred while working with transactional outbox: {e.Message}");
            throw;
        }
        await transaction.CommitAsync(cancelationToken);
    }
}
