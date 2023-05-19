using Envelope.Trace;

namespace Envelope.EntityFrameworkCore.Queries;

public abstract class QueryDefinition<TContext, T> : IQueryDefinition<TContext, T>
	where TContext : IDbContext
{
	public QueryOptions<TContext> QueryOptions { get; }

	public QueryDefinition(IServiceProvider serviceProvider)
	{
		QueryOptions = new QueryOptions<TContext>(serviceProvider);
	}

	public QueryDefinition(ContextFactory<TContext> factory)
	{
		QueryOptions = new QueryOptions<TContext>(factory);
	}

	public QueryDefinition(QueryOptions<TContext> queryOptions)
	{
		QueryOptions = queryOptions ?? throw new ArgumentNullException(nameof(queryOptions));
	}

	protected virtual Task<TContext> GetContextAsync(CancellationToken cancellationToken = default)
		=> QueryOptions.GetContextAsync(cancellationToken);

	public abstract Task<IQueryable<T>> GetQueryAsync(
		ITraceInfo traceInfo,
		CancellationToken cancellationToken = default);
}
