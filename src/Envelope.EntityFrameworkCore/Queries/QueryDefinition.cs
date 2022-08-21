using Envelope.Trace;

namespace Envelope.EntityFrameworkCore.Queries;

public abstract class QueryDefinition<TContext, T> : IQueryDefinition<TContext, T>
	where TContext : IDbContext
{
	public virtual Task<TContext> GetContextAsync(
		QueryOptions<TContext> queryOptions,
		ITraceInfo traceInfo,
		CancellationToken cancellationToken = default)
	{
		if (queryOptions == null)
			throw new ArgumentNullException(nameof(queryOptions));

		if (traceInfo == null)
			throw new ArgumentNullException(nameof(traceInfo));

		return queryOptions.GetContextAsync(cancellationToken);
	}

	public virtual Task<IQueryable<T>> GetQueryAsync(
		IServiceProvider serviceProvider,
		ITraceInfo traceInfo,
		CancellationToken cancellationToken = default)
	{
		using var queryOptions = new QueryOptions<TContext>(serviceProvider);
		return GetQueryAsync(
			queryOptions,
			traceInfo,
			cancellationToken);
	}

	public virtual Task<IQueryable<T>> GetQueryAsync(
		ContextFactory<TContext> factory,
		ITraceInfo traceInfo,
		CancellationToken cancellationToken = default)
		=> GetQueryAsync(
			new QueryOptions<TContext>(factory),
			traceInfo,
			cancellationToken);

	public abstract Task<IQueryable<T>> GetQueryAsync(
		QueryOptions<TContext> queryOptions,
		ITraceInfo traceInfo,
		CancellationToken cancellationToken = default);
}
