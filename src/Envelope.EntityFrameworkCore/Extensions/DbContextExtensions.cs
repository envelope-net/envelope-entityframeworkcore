using Envelope.EntityFrameworkCore.Queries;

namespace Envelope.EntityFrameworkCore;

public static class DbContextExtensions
{
	public static QueryOptions<TContext> CreateQueryOptions<TContext>(this TContext context)
		where TContext : IDbContext
		=> new(context);
}
