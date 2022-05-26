using Envelope.Transactions;
using Microsoft.EntityFrameworkCore.Storage;

namespace Envelope.Database.PostgreSql;

internal class DbContextTransactionBehaviorObserver : ITransactionBehaviorObserver
{
	private readonly IDbContextTransaction _transaction;

	public DbContextTransactionBehaviorObserver(IDbContextTransaction transaction)
	{
		_transaction = transaction ?? throw new ArgumentNullException(nameof(transaction));
	}

	public void Commit(ITransactionContext transactionContext)
		=> _transaction.Commit();

	public Task CommitAsync(ITransactionContext transactionContext, CancellationToken cancellationToken)
		=> _transaction.CommitAsync(cancellationToken);

	public void Rollback(ITransactionContext transactionContext, Exception? exception)
		=> _transaction.Rollback();

	public Task RollbackAsync(ITransactionContext transactionContext, Exception? exception, CancellationToken cancellationToken)
		=> _transaction.RollbackAsync(cancellationToken);

	public ValueTask DisposeAsync()
		=> _transaction.DisposeAsync();

	public void Dispose()
		=> _transaction.Dispose();
}
