using Envelope.Transactions;
using Microsoft.EntityFrameworkCore.Storage;

namespace Envelope.Database.PostgreSql;

internal class DbContextTransactionBehaviorObserver : ITransactionBehaviorObserver
{
	private bool _disposed;
	private readonly IDbContextTransaction _dbContextTransaction;

	public DbContextTransactionBehaviorObserver(IDbContextTransaction dbContextTransaction)
	{
		_dbContextTransaction = dbContextTransaction ?? throw new ArgumentNullException(nameof(dbContextTransaction));
	}

	public void Commit(ITransactionCoordinator transactionCoordinator)
		=> _dbContextTransaction.Commit();

	public Task CommitAsync(ITransactionCoordinator transactionCoordinator, CancellationToken cancellationToken)
		=> _dbContextTransaction.CommitAsync(cancellationToken);

	public void Rollback(ITransactionCoordinator transactionCoordinator, Exception? exception)
		=> _dbContextTransaction.Rollback();

	public Task RollbackAsync(ITransactionCoordinator transactionCoordinator, Exception? exception, CancellationToken cancellationToken)
		=> _dbContextTransaction.RollbackAsync(cancellationToken);

	public async ValueTask DisposeAsync()
	{
		if (_disposed)
			return;

		_disposed = true;

		await DisposeAsyncCoreAsync().ConfigureAwait(false);

		Dispose(disposing: false);
		GC.SuppressFinalize(this);
	}

	protected virtual ValueTask DisposeAsyncCoreAsync()
		=> _dbContextTransaction.DisposeAsync();

	protected virtual void Dispose(bool disposing)
	{
		if (_disposed)
			return;

		_disposed = true;

		if (disposing)
			_dbContextTransaction.Dispose();
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}
}
