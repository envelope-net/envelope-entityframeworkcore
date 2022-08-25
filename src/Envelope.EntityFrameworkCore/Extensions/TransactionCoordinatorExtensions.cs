using Envelope.Database;
using Envelope.Transactions;

namespace Envelope.EntityFrameworkCore;

public static class TransactionCoordinatorExtensions
{
	public static Task<TContext> GetOrCreateDbContextWithNewTransactionAsync<TContext>(
		this ITransactionCoordinator transactionCoordinator,
		CancellationToken cancellationToken = default)
		where TContext : IDbContext
	{
		if (transactionCoordinator == null)
			throw new ArgumentNullException(nameof(transactionCoordinator));

		return transactionCoordinator.TransactionController.GetTransactionCache<IDbContextCache>()
			.GetOrCreateIDbContextWithExistingTransactionAsync<TContext>(
				transactionCoordinator.TransactionController.GetTransactionCache<IDbTransactionFactory>(),
				transactionCoordinator,
				null,
				null,
				cancellationToken);
	}
}
