using Envelope.EntityFrameworkCore.Queries;
using Envelope.Transactions;

namespace Envelope.EntityFrameworkCore;

public static class DbContextExtensions
{
	public static QueryOptions<TContext> CreateQueryOptions<TContext>(this TContext context, ITransactionCoordinator transactionCoordinator)
		where TContext : IDbContext
		=> new(context, transactionCoordinator);
}
