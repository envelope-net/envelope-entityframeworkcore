using Envelope.EntityFrameworkCore.QueryCache;
using System.Diagnostics.CodeAnalysis;

namespace Envelope.EntityFrameworkCore;

public static partial class IQueryableGlobalCacheExtensions
{
	public static DbContextBase<TIdentity> GetDbContext<TSource, TIdentity>(this IQueryable<TSource> query)
	where TSource : class
	where TIdentity : struct
	{
		var relationalQueryContext = GetRelationalQueryContext(query);

		if (relationalQueryContext?.Context is not DbContextBase<TIdentity> dbContextBase)
			throw new NotSupportedException($"Cannot get {nameof(DbContextBase<TIdentity>)} from {query.GetType()?.FullName ?? "NULL"}");

		return dbContextBase;
	}

	public static QueryCacheManager GetDbContextQueryCacheManager<TSource, TIdentity>(this IQueryable<TSource> query)
		where TSource : class
		where TIdentity : struct
	{
		var dbContext = GetDbContext<TSource, TIdentity>(query);

		if (dbContext.QueryCacheManager == null)
			throw new NotSupportedException($"Cannot get {nameof(QueryCacheManager)} from {query.GetType()?.FullName} >> {dbContext?.GetType()?.FullName ?? "NULL"}");

		return dbContext.QueryCacheManager;
	}

	public static bool TryGetDbContext<TSource, TIdentity>(this IQueryable<TSource> query, [NotNullWhen(true)] out DbContextBase<TIdentity>? dbContextBase)
		where TSource : class
		where TIdentity : struct
	{
		var relationalQueryContext = GetRelationalQueryContext(query);
		dbContextBase = relationalQueryContext?.Context as DbContextBase<TIdentity>;
		return dbContextBase != null;
	}

	public static bool TryGetDbContextQueryCacheManager<TSource, TIdentity>(this IQueryable<TSource> query, [NotNullWhen(true)] out QueryCacheManager? queryCacheManager)
		where TSource : class
		where TIdentity : struct
	{
		if (TryGetDbContext(query, out DbContextBase<TIdentity>? dbContext))
			queryCacheManager = dbContext.QueryCacheManager;
		else
			queryCacheManager = null;

		return queryCacheManager != null;
	}
}
