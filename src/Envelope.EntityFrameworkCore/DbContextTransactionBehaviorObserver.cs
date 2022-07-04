using Envelope.Transactions;
using Microsoft.EntityFrameworkCore.Storage;

namespace Envelope.Database.PostgreSql;

internal class DbContextTransactionBehaviorObserver : ITransactionBehaviorObserver
{
	private readonly IDbContextTransaction _dbContextTransaction;

	public DbContextTransactionBehaviorObserver(IDbContextTransaction dbContextTransaction)
	{
		_dbContextTransaction = dbContextTransaction ?? throw new ArgumentNullException(nameof(dbContextTransaction));
	}

	public void Commit(ITransactionManager transactionManager)
		=> _dbContextTransaction.Commit();

	public Task CommitAsync(ITransactionManager transactionManager, CancellationToken cancellationToken)
		=> _dbContextTransaction.CommitAsync(cancellationToken);

	public void Rollback(ITransactionManager transactionManager, Exception? exception)
		=> _dbContextTransaction.Rollback();

	public Task RollbackAsync(ITransactionManager transactionManager, Exception? exception, CancellationToken cancellationToken)
		=> _dbContextTransaction.RollbackAsync(cancellationToken);

	public ValueTask DisposeAsync()
		=> _dbContextTransaction.DisposeAsync();

	public void Dispose()
		=> _dbContextTransaction.Dispose();
}
