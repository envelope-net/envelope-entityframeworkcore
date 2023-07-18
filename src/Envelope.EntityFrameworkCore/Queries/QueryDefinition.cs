using Envelope.Trace;

namespace Envelope.EntityFrameworkCore.Queries;

public abstract class QueryDefinition<TContext, T> : IQueryDefinition<TContext, T>
	where TContext : IDbContext
	where T : class
{
	public QueryOptions<TContext> QueryOptions { get; }
	public Action<QueryableBuilder<T>>? QueryableBuilder { get; }

	public QueryDefinition(IServiceProvider serviceProvider, Action<Envelope.Queries.IQueryableBuilder<T>>? queryableBuilder)
	{
		QueryOptions = new QueryOptions<TContext>(serviceProvider);
		QueryableBuilder = queryableBuilder;
	}

	public QueryDefinition(ContextFactory<TContext> factory, Action<Envelope.Queries.IQueryableBuilder<T>>? queryableBuilder)
	{
		QueryOptions = new QueryOptions<TContext>(factory);
		QueryableBuilder = queryableBuilder;
	}

	public QueryDefinition(QueryOptions<TContext> queryOptions, Action<Envelope.Queries.IQueryableBuilder<T>>? queryableBuilder)
	{
		QueryOptions = queryOptions ?? throw new ArgumentNullException(nameof(queryOptions));
		QueryableBuilder = queryableBuilder;
	}

	protected virtual Task<TContext> GetContextAsync(CancellationToken cancellationToken = default)
		=> QueryOptions.GetContextAsync(cancellationToken);

	public abstract Task<IQueryable<T>> GetQueryAsync(
		ITraceInfo traceInfo,
		CancellationToken cancellationToken = default);
}
