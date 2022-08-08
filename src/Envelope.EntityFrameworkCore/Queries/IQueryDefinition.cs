using Envelope.Trace;

namespace Envelope.EntityFrameworkCore.Queries;

public interface IQueryDefinition<TContext, T>
	where TContext : IDbContext
{
	Task<TContext> GetContextAsync(
		QueryOptions<TContext> queryOptions,
		ITraceInfo traceInfo,
		CancellationToken cancellationToken = default);

	Task<IQueryable<T>> GetQueryAsync(
		IServiceProvider serviceProvider,
		ITraceInfo traceInfo,
		CancellationToken cancellationToken = default);

	Task<IQueryable<T>> GetQueryAsync(
		QueryContextFactory<TContext> factory,
		ITraceInfo traceInfo,
		CancellationToken cancellationToken = default);

	Task<IQueryable<T>> GetQueryAsync(
		QueryOptions<TContext> queryOptions,
		ITraceInfo traceInfo,
		CancellationToken cancellationToken = default);
}
