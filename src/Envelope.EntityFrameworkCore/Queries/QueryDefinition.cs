using Envelope.EntityFrameworkCore.Extensions;
using Envelope.Trace;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

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

	protected abstract Task<IQueryable<T>> GetDefaultQueryAsync(CancellationToken cancellationToken = default);

	public abstract Task<IQueryable<T>> GetQueryAsync(
		ITraceInfo traceInfo,
		CancellationToken cancellationToken = default);

	public async Task<IQueryable<T>> WhereAsync(
		Action<QueryableBuilder<T>>? queryableBuilder,
		Expression<Func<T, bool>>? predicate,
		CancellationToken cancellationToken = default)
	{
		return predicate != null
			? (await GetDefaultQueryAsync(cancellationToken))
				.ApplyIncludes(queryableBuilder)
				.Where(predicate)
				.ApplySort(queryableBuilder)
				.ApplyPaging(queryableBuilder)
			: (await GetDefaultQueryAsync(cancellationToken))
				.ApplyIncludes(queryableBuilder)
				.ApplySort(queryableBuilder)
				.ApplyPaging(queryableBuilder);
	}

	public async Task<IQueryable<T>> ApplyQueryBuilderAsync(
		CancellationToken cancellationToken = default)
	{
		return (await GetDefaultQueryAsync(cancellationToken))
			.ApplyIncludes(QueryableBuilder)
			.ApplySort(QueryableBuilder)
			.ApplyPaging(QueryableBuilder);
	}
}
