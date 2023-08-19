using Envelope.Database;
using Envelope.EntityFrameworkCore.Queries;
using Envelope.Transactions;

namespace Envelope.EntityFrameworkCore;

public static class TransactionCoordinatorExtensions
{
	public static TContext GetOrCreateDbContextWithNewTransaction<TContext>(
		this ITransactionCoordinator transactionCoordinator,
		string connectionId)
		where TContext : IDbContext
	{
		if (transactionCoordinator == null)
			throw new ArgumentNullException(nameof(transactionCoordinator));

		return transactionCoordinator.TransactionController.GetTransactionCache<IDbContextCache>()
			.GetOrCreateIDbContextWithExistingTransaction<TContext>(
				transactionCoordinator.TransactionController.GetTransactionCache<IDbTransactionFactory>(),
				connectionId,
				transactionCoordinator,
				null,
				null);
	}

	public static QueryOptions<TContext> GetOrCreateQueryOptionsWithNewTransaction<TContext>(
		this ITransactionCoordinator transactionCoordinator,
		string connectionId)
		where TContext : IDbContext
	{
		var dbContext = GetOrCreateDbContextWithNewTransaction<TContext>(transactionCoordinator, connectionId);
		return dbContext.CreateQueryOptions(connectionId);
	}

	public static Task<TContext> GetOrCreateDbContextWithNewTransactionAsync<TContext>(
		this ITransactionCoordinator transactionCoordinator,
		string connectionId,
		CancellationToken cancellationToken = default)
		where TContext : IDbContext
	{
		if (transactionCoordinator == null)
			throw new ArgumentNullException(nameof(transactionCoordinator));

		return transactionCoordinator.TransactionController.GetTransactionCache<IDbContextCache>()
			.GetOrCreateIDbContextWithExistingTransactionAsync<TContext>(
				transactionCoordinator.TransactionController.GetTransactionCache<IDbTransactionFactory>(),
				connectionId,
				transactionCoordinator,
				null,
				null,
				cancellationToken);
	}

	public static async Task<QueryOptions<TContext>> GetOrCreateQueryOptionsWithNewTransactionAsync<TContext>(
		this ITransactionCoordinator transactionCoordinator,
		string connectionId,
		CancellationToken cancellationToken = default)
		where TContext : IDbContext
	{
		var dbContext = await GetOrCreateDbContextWithNewTransactionAsync<TContext>(transactionCoordinator, connectionId, cancellationToken);
		return dbContext.CreateQueryOptions(connectionId);
	}
}
